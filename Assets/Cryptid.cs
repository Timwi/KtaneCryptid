using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Cryptid : MonoBehaviour
{

    string ModuleName = "Cryptid";

    public KMBombModule module;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;

    int numRules;
    string solution;
    int letterCur = 0;
    int numberCur = 0;
    static Dictionary<string, List<List<string>>> spacesWithin;
    Rule[] rules;
    List<string> submitSpaces;

    public KMSelectable letterUp;
    public KMSelectable letterDown;
    public KMSelectable numberUp;
    public KMSelectable numberDown;
    public KMSelectable submit;
    public TextMesh mapSeedText;
    public TextMesh letterText;
    public TextMesh numberText;
    public MeshRenderer[] ruleMeshes;
    public Material[] ruleMats;
    public TextMesh[] ruleTexts;

    public AudioClip coordinateSFX;
    public AudioClip querySFX;
    public AudioClip solveSFX;
    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        numRules = Rnd.Range(0, 3) + 3;
        //numRules = 5;
        generatePuzzle();
    }
    void generatePuzzle()
    {
    tryagain:
        Board board = new Board(ModuleName, ModuleId);
        //board = new Board("6CB395C6A6L9G3I7H5I3H3");
        if (spacesWithin == null)
        {
            SpaceWithinDict swd = new SpaceWithinDict();
            spacesWithin = swd.spacesWithin;
        }
        RuleGenerator rg = new RuleGenerator(board, spacesWithin, numRules);
        rules = rg.rules;
        if (rules == null)
            goto tryagain;
        Debug.LogFormat("[{0} #{1}] Map Seed: {2}", ModuleName, ModuleId, board.id);
        foreach (Rule rule in rules)
        {
            Debug.LogFormat("[{0} #{1}] {2}", ModuleName, ModuleId, rule.toString());
            //Debug.LogFormat("[{0} #{1}] {2}", ModuleName, ModuleId, string.Join(" ", rule.validSpaces.ToArray()));
        }
        solution = rg.solution;
        Debug.LogFormat("[{0} #{1}] Solution: {2}", ModuleName, ModuleId, solution);
        //Generate 9 random spaces to be added to the solution
        submitSpaces = rg.generateSubmitSpaces();
        Debug.LogFormat("[{0} #{1}] Submit Spaces: {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", ModuleName, ModuleId, submitSpaces[0] + getUnfollowedRules(submitSpaces[0]), submitSpaces[1] + getUnfollowedRules(submitSpaces[1]), submitSpaces[2] + getUnfollowedRules(submitSpaces[2]), submitSpaces[3] + getUnfollowedRules(submitSpaces[3]), submitSpaces[4] + getUnfollowedRules(submitSpaces[4]), submitSpaces[5] + getUnfollowedRules(submitSpaces[5]), submitSpaces[6] + getUnfollowedRules(submitSpaces[6]), submitSpaces[7] + getUnfollowedRules(submitSpaces[7]), submitSpaces[8] + getUnfollowedRules(submitSpaces[8]), submitSpaces[9] + getUnfollowedRules(submitSpaces[9]));

        mapSeedText.text = board.id;
        for (int i = rules.Length; i < ruleMeshes.Length; i++)
            ruleMeshes[i].transform.localScale = new Vector3(0f, 0f, 0f);
        letterUp.OnInteract += delegate () { Audio.PlaySoundAtTransform(coordinateSFX.name, transform); letterCur = mod(letterCur + 1, 12); displayCoordinate(); return false; };
        letterDown.OnInteract += delegate () { Audio.PlaySoundAtTransform(coordinateSFX.name, transform); letterCur = mod(letterCur - 1, 12); displayCoordinate(); return false; };
        numberUp.OnInteract += delegate () { Audio.PlaySoundAtTransform(coordinateSFX.name, transform); numberCur = mod(numberCur + 1, 9); displayCoordinate(); return false; };
        numberDown.OnInteract += delegate () { Audio.PlaySoundAtTransform(coordinateSFX.name, transform); numberCur = mod(numberCur - 1, 9); displayCoordinate(); return false; };
        submit.OnInteract += delegate () { pressedSubmit(); return false; };
        displayCoordinate();
    }
    private string getUnfollowedRules(string space)
    {
        string unfollowedRules = "";
        for(int i = 0; i < rules.Length; i++)
        {
            if (!rules[i].validSpaces.Contains(space))
                unfollowedRules += (i + 1);
        }
        if (unfollowedRules.Length == 0)
            unfollowedRules = "S";
        return "(" + unfollowedRules + ")";
    }
    void displayCoordinate()
    {
        letterText.text = "ABCDEFGHIJKL"[letterCur] + "";
        numberText.text = (numberCur + 1) + "";
        string coord = letterText.text + numberText.text;
        if (submitSpaces.Contains(coord))
        {
            for (int i = 0; i < numRules; i++)
            {
                ruleMeshes[i].material = ruleMats[ruleMats.Length - 1];
                ruleTexts[i].text = "";
            }
        }
        else
        {
            for (int i = 0; i < numRules; i++)
            {
                ruleMeshes[i].material = ruleMats[i];
                ruleTexts[i].text = rules[i].validSpaces.Contains(coord) ? "O" : "X";
            }
        }
    }
    void pressedSubmit()
    {
        string coord = letterText.text + numberText.text;
        if (submitSpaces.Contains(coord))
        {
            Debug.LogFormat("[{0} #{1}] User submitted {2}", ModuleName, ModuleId, coord);
            if (coord.Equals(solution))
                Solve();
            else
                Strike();
        }
    }
    void Solve()
    {
        Audio.PlaySoundAtTransform(solveSFX.name, transform);
        letterUp.OnInteract = null;
        letterDown.OnInteract = null;
        numberUp.OnInteract = null;
        numberDown.OnInteract = null;
        submit.OnInteract = null;
        letterText.text = "";
        numberText.text = "";
        mapSeedText.text = "";
        module.HandlePass();
    }

    void Strike()
    {
        Debug.LogFormat("[{0} #{1}] That was the wrong space! Time to reveal which rule(s) wasn't satisfied!", ModuleName, ModuleId);
        module.HandleStrike();
        string coord = letterText.text + numberText.text;
        submitSpaces.Remove(coord);
        displayCoordinate();
    }

    
    int mod(int n, int m)
    {
        while (n < 0)
            n += m;
        return (n % m);
    }

    bool TPautosolve = false;
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} (C)ycle [ABCDEFGHIJKL123456789] will cycle through the coordinates that contains those letters/numbers. !{0} (T)ile F3 will go to the F3 space. !{0} (S)ubmit G7 will submit the G7 space. !{0} (S)ubmit will retrieve all spaces that are black.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if(!TPautosolve)
        {
            string[] param = command.ToUpper().Split(' ');
            if ((Regex.IsMatch(param[0], @"^\s*CYCLE\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*C\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) && param.Length == 2 && isValidCoordCycle(param[1]))
            {
                yield return null;
                string letters = "", numbers = "";
                foreach(char cha in param[1])
                {
                    if ("123456789".Contains(cha))
                    {
                        if(!numbers.Contains(cha))
                            numbers += cha;
                    }
                    else
                    {
                        if (!letters.Contains(cha))
                            letters += cha;
                    } 
                }
                foreach(char letter in letters)
                {
                    yield return "trycancel Cycling has been cancelled due to a cancel request.";
                    while (letterText.text[0] != letter)
                    {
                        letterUp.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    foreach (char number in numbers)
                    {
                        yield return "trycancel Cycling has been cancelled due to a cancel request.";
                        while (numberText.text[0] != number)
                        {
                            numberUp.OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        yield return new WaitForSeconds(1f);
                    }
                }
            }
            else if ((Regex.IsMatch(param[0], @"^\s*TILE\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*S\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) && param.Length == 2 && isValidSpace(param[1]))
            {
                yield return null;
                while (letterText.text[0] != param[1][0])
                {
                    letterUp.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                while (numberText.text[0] != param[1][1])
                {
                    numberUp.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if ((Regex.IsMatch(param[0], @"^\s*SUBMIT\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*S\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) && param.Length == 2 && isValidSpace(param[1]))
            {
                yield return null;
                while (letterText.text[0] != param[1][0])
                {
                    letterUp.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                while (numberText.text[0] != param[1][1])
                {
                    numberUp.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                submit.OnInteract();
            }
            else if ((Regex.IsMatch(param[0], @"^\s*SUBMIT\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*S\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) && param.Length == 1)
            {
                yield return null;
                string str = "";
                foreach (string space in submitSpaces)
                    str += space + " ";
                yield return "sendtochat " + str;
            }
            else
                yield return "sendtochat An error occured because the user inputted something wrong.";
        }
        else
            yield return "sendtochat Module is being solved at the moment.";
        yield return null;
    }
    bool isValidSpace(string input)
    {
        if(input.Length == 2)
            return "ABCDEFGHIJKL".Contains(input[0]) && "123456789".Contains(input[1]);
        return false;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        TPautosolve = true;
        yield return null;
        while (letterText.text[0] != solution[0])
        {
            letterUp.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while (numberText.text[0] != solution[1])
        {
            numberUp.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        submit.OnInteract();
    }
    bool isValidCoordCycle(string str)
    {
        bool b1 = false, b2 = false;
        foreach(char cha in str)
        {
            if (!"ABCDEFGHIJKL123456789".Contains(cha))
                return false;
            else if ("123456789".Contains(cha))
                b1 = true;
            else
                b2 = true;
        }
        return b1 && b2;
    }
}
