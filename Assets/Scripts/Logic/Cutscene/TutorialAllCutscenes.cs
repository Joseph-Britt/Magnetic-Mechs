using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAllCutscenes : MonoBehaviour
{
    //a file which holds all of the dialgoue, images and sounds used in cutscenes
    public Dictionary<string,Dialogue> cutscenes;
    private Dictionary<string, string> imageLocations;
    private Dictionary<string, string> audioLocations;
    [Header("Dialogues")]
    private Dialogue firstCutscene;
    private Dialogue magnetCutscene;
    private Dialogue lastCutscenePartOne;
    private Dialogue lastCutscenePartTwo;
    private Dialogue lastCutscenePartThree;
    private Dialogue lastCutscenePartFour;
    [Header("Components")]
    public GameObject dialogueBox;
    public GameObject dialogueText;
    public GameObject dialogueImage;
    public GameObject dialogueAudio;
    

    // Start is called before the first frame update
    void Awake()
    {
        //set up images
        imageLocations = new Dictionary<string, string>
        {
            { "General", "DialogueImages/Shoot__000" },
            { "Player", "DialogueImages/Walk__009" },
        };
        //set up audio
        audioLocations = new Dictionary<string, string>
        {
            { "General", "DialogueAudio/DM-CGS-38" },
            { "Player", "DialogueAudio/High-Beep" }
        };
        //set up cutscenes
        cutscenes = new Dictionary<string, Dialogue>();

        //first cutscene
        firstCutscene = gameObject.AddComponent<Dialogue>();
        firstCutscene.postHocConstructor(
        "First Cutscene",
        new DialogueIndividualLine("Alright Soldiers, your job here is to identify and eliminate any hostiles in the immediate area", imageLocations["General"], audioLocations["General"], 2),
        new DialogueIndividualLine("Direct your mech to move right and proceed", imageLocations["General"], audioLocations["General"], 2)
        );
        cutscenes.Add("First Cutscene", firstCutscene);

        //magnet cutscene
        magnetCutscene = gameObject.AddComponent<Dialogue>();
        magnetCutscene.postHocConstructor(
        "Magnet Cutscene",
        new DialogueIndividualLine("Sir, we've encountered a gap too large for our jetpacks to get over it", imageLocations["Player"], audioLocations["Player"], 2),
        new DialogueIndividualLine("Your mechs are equipped with state of the art magnetic technology for bypassing such obstacles", imageLocations["General"], audioLocations["General"], 2),
        new DialogueIndividualLine("If you just tell your mechs to proceed, I'm sure they'll figure out a way to make it happen", imageLocations["General"], audioLocations["General"], 2)
        );
        cutscenes.Add("Magnet Cutscene", magnetCutscene);

        //Finale Part One Cutscene
        lastCutscenePartOne = gameObject.AddComponent<Dialogue>();
        lastCutscenePartOne.postHocConstructor(
        "Last Cutscene Part One",
        new DialogueIndividualLine("Sir, multiple hostiles identified", imageLocations["Player"], audioLocations["Player"], 2),
        new DialogueIndividualLine("Excellent work soldier, neutralize them", imageLocations["General"], audioLocations["General"], 2)
        );
        cutscenes.Add("Last Cutscene Part One", lastCutscenePartOne);

        //Last Cutscene Part Two
        lastCutscenePartTwo = gameObject.AddComponent<Dialogue>();
        lastCutscenePartTwo.postHocConstructor(
        "Last Cutscene Part Two",
        new DialogueIndividualLine("Sir, my controls are completely fried. The Mechs not listening to me at all", imageLocations["Player"], audioLocations["Player"], 2)
        );
        cutscenes.Add("Last Cutscene Part Two", lastCutscenePartTwo);

        //Last Cutscene Part Three
        lastCutscenePartThree = gameObject.AddComponent<Dialogue>();
        lastCutscenePartThree.postHocConstructor(
        "Last Cutscene Part Three",
        new DialogueIndividualLine("Wait, what's this thing -", imageLocations["Player"], audioLocations["Player"], 2)
        );
        cutscenes.Add("Last Cutscene Part Three", lastCutscenePartThree);

        //Last Cutscene Part Four
        lastCutscenePartFour = gameObject.AddComponent<Dialogue>();
        lastCutscenePartFour.postHocConstructor(
        "Last Cutscene Part Four",
        new DialogueIndividualLine("...", imageLocations["Player"], "", 3)
        );
        cutscenes.Add("Last Cutscene Part Four", lastCutscenePartFour);

        //grab components
        dialogueBox = GameObject.FindGameObjectWithTag("DialogueBox");
        dialogueImage = GameObject.FindGameObjectWithTag("DialogueImage");
        dialogueText = GameObject.FindGameObjectWithTag("DialogueText");
        dialogueAudio = GameObject.FindGameObjectWithTag("DialogueAudio");
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }
}
