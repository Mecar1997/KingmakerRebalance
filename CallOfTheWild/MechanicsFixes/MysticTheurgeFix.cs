﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    class MysticTheurgeFix
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass mystic_theurge = library.Get<BlueprintCharacterClass>("0920ea7e4fd7a404282e3d8b0ac41838");
        static BlueprintCharacterClass magus_class = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
        static BlueprintProgression mystic_theurge_progression = library.Get<BlueprintProgression>("08c1075ef2786ef4fae11e82698a16e0");
        static BlueprintProgression magus_progression = library.Get<BlueprintProgression>("1b912721a7e075d4f9cfe8dafa39414c");
        static BlueprintProgression sorcerer_progression = library.Get<BlueprintProgression>("997665565ca80a649aedd72455c4df1f");
        static BlueprintProgression wizard_progression = library.Get<BlueprintProgression>("02f3049806dbf62459259ea8cae8f715");
        static BlueprintProgression cleric_progression = library.Get<BlueprintProgression>("b2cd67193d1199f41bc6ecec3a2f2c87");
        static BlueprintProgression inquisitor_progression = library.Get<BlueprintProgression>("4e945c2fe5e252f4ea61eee7fb560017");
        static BlueprintProgression druid_progression = library.Get<BlueprintProgression>("01006f2ac8866764fb7af135e73be81c");
        static BlueprintProgression ranger_progression = library.Get<BlueprintProgression>("97261d609529d834eba4fd4da1bc44dc");
        static BlueprintProgression paladin_progression = library.Get<BlueprintProgression>("fd325cbba872e5f40b618970678db002");
        static BlueprintProgression bard_progression = library.Get<BlueprintProgression>("8127f5ff40f5b484b8be98609358b9d2");
        static BlueprintArchetype eldritch_scoundrel = library.Get<BlueprintArchetype>("57f93dd8423c97c49989501281296c4a");
        
        static BlueprintFeature spell_synthesis;
        static BlueprintFeature extra_spell_synthesis;
        static BlueprintFeature lesser_spell_synthesis;
        static BlueprintFeature extra_lesser_spell_synthesis;
        static BlueprintFeature theurgy;
        static public BlueprintFeature spell_specialization;


        public static void load()
        {
            createSpellSynthesis();
            createLesserSpellSynthesis();
            createTheurgy();
            changeMagusProgression();
            changeDivineGuardianSpellbook();
            addSpellSpecialization();
        }


        static void createLesserSpellSynthesis()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/FontOfSpiritMagic.png");
            var combined_spells = library.Get<BlueprintFeature>("80ea00ac94323cd43b6e743f2fa168c8");
            lesser_spell_synthesis = Helpers.CreateFeature("LesserSpellSynthesisFeature",
                                            "Lesser Spell Synthesis",
                                            "Once per day as a full-round action, you can cast two spells, each from a different spellcasting class. Both spells must have a casting time of 1 standard action and must be a spell level equal to or lower than the level of spells you can prepare with the combined spells ability. You can make any decisions concerning the spells, such as the spells’ targets, independently.",
                                            "",
                                            icon,
                                            FeatureGroup.Feat,
                                            Helpers.PrerequisiteFeature(combined_spells));

            var resource = Helpers.CreateAbilityResource("LesserSpellSynthesisResource", "", "", "", null);
            resource.SetFixedResource(1);

            var arcane_buff = Helpers.CreateBuff($"LesserSpellSynthesisArcaneBuff",
                                      lesser_spell_synthesis.Name + " (Arcane)",
                                      lesser_spell_synthesis.Description,
                                      "",
                                      icon,
                                      null,
                                      Helpers.Create<TurnActionMechanics.SpellSynthesis>(s =>
                                                                                                  {
                                                                                                      s.action_type = CommandType.Standard;
                                                                                                      s.is_full_round = false;
                                                                                                      s.max_spell_level = Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default);
                                                                                                      s.is_arcane = true;
                                                                                                  }
                                                                                                  ),
                                      Helpers.CreateContextRankConfig(baseValueType: Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                  classes: new BlueprintCharacterClass[] { mystic_theurge },
                                                                  progression: Kingmaker.UnitLogic.Mechanics.Components.ContextRankProgression.OnePlusDiv2
                                                                  )
                                      );

            var divine_buff = Helpers.CreateBuff($"LesserSpellSynthesisDivineBuff",
                                  lesser_spell_synthesis.Name + " (Divine)",
                                  lesser_spell_synthesis.Description,
                                  "",
                                  icon,
                                  null,
                                  Helpers.Create<TurnActionMechanics.SpellSynthesis>(s =>
                                                                                              {
                                                                                                  s.action_type = CommandType.Standard;
                                                                                                  s.is_full_round = false;
                                                                                                  s.max_spell_level = Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default);
                                                                                                  s.is_arcane = false;
                                                                                              }
                                                                                              ),
                                  Helpers.CreateContextRankConfig(baseValueType: Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                  classes: new BlueprintCharacterClass[] {mystic_theurge},
                                                                  progression: Kingmaker.UnitLogic.Mechanics.Components.ContextRankProgression.OnePlusDiv2
                                                                  )
                                  );


            var buff = Helpers.CreateBuff("LesserSpellSynthesisBuff",
                                          lesser_spell_synthesis.Name,
                                          lesser_spell_synthesis.Description,
                                          "",
                                          icon,
                                          null
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var ability = Helpers.CreateAbility($"LesserSpellSynthesisAbility",
                                    "Lesser Spell Synthesis",
                                    lesser_spell_synthesis.Description,
                                    "",
                                    icon,
                                    Kingmaker.UnitLogic.Abilities.Blueprints.AbilityType.Supernatural,
                                    CommandType.Standard,
                                    Kingmaker.UnitLogic.Abilities.Blueprints.AbilityRange.Personal,
                                    Helpers.oneRoundDuration,
                                    "",
                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(arcane_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                             Common.createContextActionApplyBuff(divine_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                             Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                             ),
                                    Helpers.CreateResourceLogic(resource)
                                    );
            ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(ability);

            lesser_spell_synthesis.AddComponents(Helpers.CreateAddFact(ability),
                                                Helpers.CreateAddAbilityResource(resource));


            extra_lesser_spell_synthesis = Helpers.CreateFeature("ExtraLesserSpellSynthesisFeature",
                                              "Extra Lesser Spell Synthesis",
                                              "You can perform one additional lesser spell synthesis per day.\n"
                                              + "Special: You can select this feat multiple times. Each time you take the feat, you can use this ability one additional time per day.",
                                              "",
                                              icon,
                                              FeatureGroup.Feat,
                                              Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c => { c.Resource = resource; c.Value = Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default); }),
                                              Helpers.PrerequisiteFeature(combined_spells),
                                              Helpers.PrerequisiteFeature(lesser_spell_synthesis)
                                              );
            extra_lesser_spell_synthesis.AddComponent(Helpers.CreateContextRankConfig(baseValueType: Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.FeatureRank,
                                                                                          feature: extra_lesser_spell_synthesis)
                                              );
            extra_lesser_spell_synthesis.ReapplyOnLevelUp = true;
            extra_lesser_spell_synthesis.Ranks = 10;

            library.AddFeats(lesser_spell_synthesis, extra_lesser_spell_synthesis);
        }


        static void createTheurgy()
        {
            var feat_icon = LoadIcons.Image2Sprite.Create(@"FeatIcons/Theurgy.png");
            var feat_name = "Theurgy";
            var feat_description = " You can augment the power of your divine spells with arcane energy and augment your arcane spells with divine energy.\n"
                                  + "You may sacrifice an arcane spell slot or arcane prepared spell as a swift action to increase caster level of next divine spell of the same level or lower you will cast in this round.\n"
                                  + "You may sacrifice a divine spell slot or divine prepared spell as a swift action to increase caster level of next arcane spell of the same level or lower you will cast in this round.\n"
                                  + "In addition this feat allows to reduce spell level requirement to 1st-level spells for one of the classes when qualifying for Mystic Theurge prestige class.";

            var arcane_buffs_list = new List<BlueprintBuff>();
            var divine_buffs_list = new List<BlueprintBuff>();

            for (int l = 1; l < 10; l++)
            {
                var arcane_buff = Helpers.CreateBuff($"TheurgyArcane{l}Buff",
                                                 feat_name + $": Arcane Spells Caster Level Bonus ({l})",
                                                 feat_description,
                                                 "",
                                                 feat_icon,
                                                 null,
                                                 Helpers.Create<SpellManipulationMechanics.IncreaseSpellTypeCasterLevel>(i => { i.apply_to_arcane = true; i.bonus = 1; i.remove_after_use = true; i.max_lvl = l; })
                                                 );
                arcane_buff.Stacking = StackingType.Replace;
                var divine_buff = Helpers.CreateBuff($"TheurgyDivine{l}Buff",
                                                     feat_name + $": Divine Spells Caster Level Bonus ({l})",
                                                     feat_description,
                                                     "",
                                                     feat_icon,
                                                     null,
                                                     Helpers.Create<SpellManipulationMechanics.IncreaseSpellTypeCasterLevel>(i => { i.apply_to_divine = true; i.bonus = 1; i.remove_after_use = true; i.max_lvl = l; })
                                                     );
                divine_buff.Stacking = StackingType.Replace;
                arcane_buffs_list.Add(arcane_buff);
                divine_buffs_list.Add(divine_buff);
            }

            var abilities = new List<BlueprintAbility>();
            theurgy = Helpers.CreateFeature("TheurgyFeature",
                                feat_name,
                                feat_description,
                                "",
                                feat_icon,
                                FeatureGroup.Feat,
                                Helpers.PrerequisiteStatValue(StatType.Wisdom, 13),
                                Helpers.PrerequisiteStatValue(StatType.Intelligence, 13, any: true),
                                Helpers.PrerequisiteStatValue(StatType.Charisma, 13, any: true),
                                Common.createPrerequisiteCasterTypeSpellLevel(true, 1),
                                Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 1; })
                                );

            for (int i = 1; i < 10; i++)
            {
                int x = i;
                Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
                {
                    if (spell.Spellbook == null)
                    {
                        return false;
                    }
                    if (spell.SpellLevel != x)
                    {
                        return false;
                    }
                    return spell.Spellbook.Blueprint.IsArcane ||
                           !(spell.Spellbook.Blueprint.IsAlchemist || spell.Spellbook.Blueprint.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null);
                };

                var apply_divine = Common.createContextActionApplyBuff(divine_buffs_list[i-1], Helpers.CreateContextDuration(1), dispellable: false);
                var apply_arcane = Common.createContextActionApplyBuff(arcane_buffs_list[i-1], Helpers.CreateContextDuration(1), dispellable: false);
                var ability = Helpers.CreateAbility($"Theurgy{i}Ability",
                                                    feat_name,
                                                    feat_description,
                                                    "",
                                                    feat_icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    Helpers.oneRoundDuration,
                                                    "",
                                                    Helpers.Create<SpellManipulationMechanics.RunActionOnTargetBasedOnSpellType>(r =>
                                                    {
                                                        r.action_arcane = Helpers.CreateActionList(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = divine_buffs_list.ToArray()),
                                                                                                   apply_divine);
                                                        r.action_divine = Helpers.CreateActionList(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = arcane_buffs_list.ToArray()),
                                                                                                   apply_arcane);
                                                        r.check_slot_predicate = check_slot_predicate;
                                                        r.context_fact = theurgy;
                                                    })
                                                    );
                abilities.Add(ability);
            }

            theurgy.AddComponents(Helpers.CreateAddFacts(abilities.ToArray()));
            library.AddFeats(theurgy);

            mystic_theurge.RemoveComponents<PrerequisiteCasterTypeSpellLevel>();
            mystic_theurge.AddComponents(Helpers.Create<PrerequisiteMechanics.CompoundPrerequisites>(p =>
                                        {
                                            p.prerequisites = new Prerequisite[]
                                            {
                                                Common.createPrerequisiteCasterTypeSpellLevel(true, 2),
                                                Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(pp => { pp.RequiredSpellLevel = 2; })
                                            };
                                            p.Group = Prerequisite.GroupType.Any;
                                        }),
                                        Helpers.Create<PrerequisiteMechanics.CompoundPrerequisites>(p =>
                                        {
                                            p.prerequisites = new Prerequisite[]
                                            {
                                                Common.createPrerequisiteCasterTypeSpellLevel(true, 2),
                                                Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(pp => { pp.RequiredSpellLevel = 1; }),
                                                Helpers.PrerequisiteFeature(theurgy)
                                            };
                                            p.Group = Prerequisite.GroupType.Any;
                                        }),
                                        Helpers.Create<PrerequisiteMechanics.CompoundPrerequisites>(p =>
                                        {
                                            p.prerequisites = new Prerequisite[]
                                            {
                                                Common.createPrerequisiteCasterTypeSpellLevel(true, 1),
                                                Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(pp => { pp.RequiredSpellLevel = 2; }),
                                                Helpers.PrerequisiteFeature(theurgy)
                                            };
                                            p.Group = Prerequisite.GroupType.Any;
                                        })
                                        );

            var arcane_spellbook_selection = library.Get<BlueprintFeatureSelection>("97f510c6483523c49bc779e93e4c4568");
            var divine_spellbook_selection = library.Get<BlueprintFeatureSelection>("7cd057944ce7896479717778330a4933");

            foreach (var sb in arcane_spellbook_selection.AllFeatures)
            {
                sb.ReplaceComponent<PrerequisiteClassSpellLevel>(p => p.RequiredSpellLevel = 1);
            }
            foreach (var sb in divine_spellbook_selection.AllFeatures)
            {
                sb.ReplaceComponent<PrerequisiteClassSpellLevel>(p => p.RequiredSpellLevel = 1);
            }
        }


        static void createSpellSynthesis()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png");

            spell_synthesis = Helpers.CreateFeature("SpellSynthesisFeature",
                                                        "Spell Synthesis",
                                                        "At 10th level, a mystic theurge can cast two spells, one from each of his spellcasting classes, using one action. Both of the spells must have the same casting time. The mystic theurge can make any decisions concerning the spells independently.Any target affected by both of the spells takes a –2 penalty on saves made against each spell.The mystic theurge receives a + 2 bonus on caster level checks made to overcome spell resistance with these two spells. A mystic theurge may use this ability once per day.",
                                                        "",
                                                        icon,
                                                        FeatureGroup.None);

            var buff = Helpers.CreateBuff("SpellSynthesisBuff",
                                          spell_synthesis.Name,
                                          spell_synthesis.Description,
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<NewMechanics.IncreaseAllSpellsDC>(i => i.Value = 2),
                                          Helpers.Create<SpellPenetrationBonus>(s => s.Value = 2)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);


            CommandType[] action_types = new CommandType[] { CommandType.Standard, CommandType.Standard, CommandType.Swift };
            bool[] full_round = new bool[] { true, false, false };
            string[] names = new string[] { "Full Round Action", "Standard Action", "Swift Action" };
            List<BlueprintAbility> abilities = new List<BlueprintAbility>();

            var resource = Helpers.CreateAbilityResource("SpellSynthesisResource", "", "", "", null);
            resource.SetFixedResource(1);

            for (int i = 0; i < action_types.Length; i++)
            {
                var arcane_buff = Helpers.CreateBuff($"SpellSynthesis{i}ArcaneBuff",
                                                      "Spell Synthesis (Arcane): " + names[i],
                                                      spell_synthesis.Description,
                                                      "",
                                                      icon,
                                                      null,
                                                      Helpers.Create<TurnActionMechanics.SpellSynthesis>(s => 
                                                                                                                  { s.action_type = action_types[i];
                                                                                                                    s.is_full_round = full_round[i];
                                                                                                                    s.max_spell_level = 100;
                                                                                                                    s.is_arcane = true;
                                                                                                                  }
                                                                                                                  )
                                                      );

                var divine_buff = Helpers.CreateBuff($"SpellSynthesis{i}DivineBuff",
                                      "Spell Synthesis (Divine): " + names[i],
                                      spell_synthesis.Description,
                                      "",
                                      icon,
                                      null,
                                      Helpers.Create<TurnActionMechanics.SpellSynthesis>(s =>
                                                                                                  {
                                                                                                      s.action_type = action_types[i];
                                                                                                      s.is_full_round = full_round[i];
                                                                                                      s.max_spell_level = 100;
                                                                                                      s.is_arcane = false;
                                                                                                  }
                                                                                                  )
                                      );

                var ability = Helpers.CreateAbility($"SpellSynthesis{i}Ability",
                                                    "Spell Synthesis: " + names[i],
                                                    spell_synthesis.Description,
                                                    "",
                                                    icon,
                                                    Kingmaker.UnitLogic.Abilities.Blueprints.AbilityType.Supernatural,
                                                    action_types[i],
                                                    Kingmaker.UnitLogic.Abilities.Blueprints.AbilityRange.Personal,
                                                    Helpers.oneRoundDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(arcane_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                                             Common.createContextActionApplyBuff(divine_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                                             Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                                             ),
                                                    Helpers.CreateResourceLogic(resource)
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                if (full_round[i])
                {
                    Common.setAsFullRoundAction(ability);
                }

                abilities.Add(ability);
            }

            var base_ability = Common.createVariantWrapper("SpellSynthesisAbility", "", abilities.ToArray());
            base_ability.SetName(spell_synthesis.Name);

            spell_synthesis.AddComponents(Helpers.CreateAddFact(base_ability),
                                          Helpers.CreateAddAbilityResource(resource));

            mystic_theurge_progression.LevelEntries[9].Features.Add(spell_synthesis);

            extra_spell_synthesis = Helpers.CreateFeature("ExtraSpellSynthesisFeature",
                                                          "Extra Spell Synthesis",
                                                          "You can perform one additional spell synthesis per day.\n"
                                                          + "Special: You can select this feat multiple times. Each time you take the feat, you can use this ability one additional time per day.",
                                                          "",
                                                          icon,
                                                          FeatureGroup.Feat,
                                                          Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c => { c.Resource = resource; c.Value = Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default); }),
                                                          Helpers.PrerequisiteFeature(spell_synthesis)
                                                          );
            extra_spell_synthesis.AddComponent(Helpers.CreateContextRankConfig(baseValueType: Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.FeatureRank,
                                                                                          feature: extra_spell_synthesis)
                                              );
            extra_spell_synthesis.ReapplyOnLevelUp = true;
            extra_spell_synthesis.Ranks = 10;

            library.AddFeats(extra_spell_synthesis);
        }

        static void changeMagusProgression()
        {
            var magus_spellstrike = library.Get<BlueprintFeature>("be50f4e97fff8a24ba92561f1694a945");
            var magus_arcana = library.Get<BlueprintFeature>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var magus_spellcombat = library.Get<BlueprintFeature>("2464ba53317c7fc4d88f383fac2b45f9");
            var magus_counterstrike = library.Get<BlueprintFeature>("cd96b7275c206da4899c69ae127ffda6");


            magus_progression.LevelEntries[0].Features.Add(magus_spellstrike);
            magus_progression.LevelEntries[1].Features.Remove(magus_spellstrike);  


            // Eldritch Archer
            var eldritch_archer = library.Get<BlueprintArchetype>("44388c01eb4a29d4d90a25cc0574320d");
            var eldritch_archer_spellstrike = library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc");
            var eldritch_archer_spellcombat = library.Get<BlueprintFeature>("8b68a5b8223beed40b137885116c408f");

            eldritch_archer.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, magus_spellstrike, magus_spellcombat),
                                                          Helpers.LevelEntry(3, magus_arcana),
                                                          Helpers.LevelEntry(16, magus_counterstrike),
                                                       };

            eldritch_archer.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, eldritch_archer_spellcombat, eldritch_archer_spellstrike, magus_arcana),

                                                       };
        }

        static void changeDivineGuardianSpellbook()
        {
            var divineguardian = library.Get<BlueprintArchetype>("5693945afac189a469ef970eac8f71d9");
            var layOnHandsFeature = library.Get<BlueprintFeature>("858a3689c285c844d9e6ce278e686491");
            var auraOfCourageFeature = library.Get<BlueprintFeature>("e45ab30f49215054e83b4ea12165409f");
            divineguardian.RemoveSpellbook = false;
            divineguardian.ChangeCasterType = false;

            divineguardian.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, layOnHandsFeature),
                                                          Helpers.LevelEntry(3, auraOfCourageFeature),
                                                       };
        }

        static void addSpellSpecialization()
        {
            spell_specialization = Helpers.CreateFeature("MentalDexterity",
                                            "Mental Dexterity",
                                        "Casters may their bonus from Intelligence, Wisdom or Charisma (the highest stat is chosen) if it is higher than their Dexterity or Strength bonus for calculating the attack roll of Touch and Ray spells.",
                                        "",
                                        null,
                                        FeatureGroup.None,
                                        Helpers.Create<NewMechanics.AttackStatReplacementForCasters>(c =>
                                        {
                                            c.categories = new WeaponCategory[] { WeaponCategory.Touch, WeaponCategory.Ray };

                                        }
                                        )
                                        );

            magus_progression.LevelEntries[0].Features.Add(spell_specialization);
            sorcerer_progression.LevelEntries[0].Features.Add(spell_specialization);
            wizard_progression.LevelEntries[0].Features.Add(spell_specialization);
            cleric_progression.LevelEntries[0].Features.Add(spell_specialization);
            inquisitor_progression.LevelEntries[0].Features.Add(spell_specialization);
            druid_progression.LevelEntries[0].Features.Add(spell_specialization);
            ranger_progression.LevelEntries[0].Features.Add(spell_specialization);
            paladin_progression.LevelEntries[0].Features.Add(spell_specialization);
            bard_progression.LevelEntries[0].Features.Add(spell_specialization);
        }

        static BlueprintAbility acid_splash = library.Get<BlueprintAbility>("0c852a2405dd9f14a8bbcfaf245ff823");

        static void cantripScaling()
        {

            acid_splash.AddComponent(Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDice, max: 15, feature: MetamagicFeats.intensified_metamagic));


            
        }
    }
}
