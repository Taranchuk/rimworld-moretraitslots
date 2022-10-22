using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace MoreTraitSlots
{
    [StaticConstructorOnStartup]
    internal class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("com.rimworld.mod.moretraitslots");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Log.Message(
            //    "MoreTraitSlots Harmony Patches:" + Environment.NewLine +
            //    "  Prefix:" + Environment.NewLine +
            //    "    PawnGenerator.GenerateTraits [HarmonyPriority(Priority.VeryHigh)]"/* + Environment.NewLine +
            //    "    CharacterCardUtility.DrawCharacterCard [HarmonyPriority(Priority.VeryHigh)]"*/);
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
    internal static class PawnGenerator_GenerateTraits
    {
        private static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            int traitCount = Rand.RangeInclusive((int)RMTS.Settings.traitsMin, (int)RMTS.Settings.traitsMax) - pawn.story.traits.allTraits.Count;
            while (traitCount > pawn.story.traits.allTraits.Count)
            {
                Trait trait = PawnGenerator.GenerateTraitsFor(pawn, 1, request, growthMomentTrait: false).FirstOrFallback();
                if (trait != null)
                {
                    pawn.story.traits.GainTrait(trait);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraitsFor")]
    internal static class PawnGenerator_GenerateTraitsFor
    {
        private static void Prefix(Pawn pawn, ref int traitCount, PawnGenerationRequest? req = null, bool growthMomentTrait = false)
        {
            traitCount = Rand.RangeInclusive((int)RMTS.Settings.traitsMin, (int)RMTS.Settings.traitsMax) - pawn.story.traits.allTraits.Count;
            if (pawn.story.traits.allTraits.Count >= traitCount)
            {
                traitCount = 0;
            }
        }
    }

    /*[HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard", new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) })]
    static class CharacterCardUtility_DrawCharacterCard
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo SetTextSize = AccessTools.Method(typeof(CharacterCardUtility_DrawCharacterCard), nameof(SetTextSize));
            List<CodeInstruction> l = new List<CodeInstruction>(instructions);
            for (int i = 0; i < l.Count; ++i)
            {
                if (l[i].opcode == OpCodes.Ldstr && l[i].operand.Equals("Traits"))
                {
                    for (int j = i; j >= i - 20; --j)
                    {
                        if (l[j].opcode == OpCodes.Ldc_R4)
                        {
                            float temp;
                            if (float.TryParse(l[j].operand.ToString(), out temp))
                            {
                                if (temp == 100f)
                                {
                                    // Move "Traits" up 60 pixels
                                    l[j].operand = 80f;
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = i; i >= i - 20; --j)
                    {
                        if (l[j].opcode == OpCodes.Ldc_I4_2)
                        {
                            // Make Traits font small
                            l[j].opcode = OpCodes.Ldc_I4_1;
                            break;
                        }
                    }

                    bool first0 = false,
                         first30 = false,
                         first24 = false;
                    for (; i < l.Count; ++i)
                    {
                        if (l[i].opcode == OpCodes.Ldc_R4 && l[i].operand != null)
                        {
                            float f;
                            if (float.TryParse(l[i].operand.ToString(), out f))
                            {
                                if (!first30 && f == 30f)
                                {
                                    //first30 = true;
                                    l[i].operand = 24f;
                                }
                                else if (!first24 && f == 24f)
                                {
                                    first24 = true;
                                    l[i].operand = 16f;
                                    break;
                                }
                            }
                        }
                        else if (!first0 && l[i].opcode == OpCodes.Ldc_I4_1)
                        {
                            first0 = true;
                            // Make each trait's label font tiny
                            l[i].opcode = OpCodes.Ldc_I4_0;
                        }
                    }
                    // Exit loop
                    i = int.MaxValue - 1;
                }
            }
            return l;
        }
    }*/
}