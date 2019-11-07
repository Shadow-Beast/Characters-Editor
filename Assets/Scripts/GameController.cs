using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject subOptionPanel, editorModeActionTypePanel, previewModeActionTypePanel, actionListPanel, messagePanel, blankPanel, warningPanel, savePanel, loadPanel, subOptionButtonPrefab, actionPanelPrefab;
    public Text subOptionTitle, txtPreviewButton, txtInformation, txtMessage;
    public Button btnBody, btnTexture, btnAction, btnSave, btnPreviewEditor;
    public Color normalColor, clickedColor;
    public Transform subOptionContent, actionListContent;
    public Image actionPanelActionButtonImage;

    private Button oldClickedButton;
    private Enums.SubOptionButtons oldClickedButtonName;
    private bool isPreview, isModelChosen, letPlay;
    private Character currentCharacter;
    private AnimationClip[][] animationList;
    private GameObject[] bodyList;
    private GameObject lastChosenBody;
    private Texture[] textureList;
    private int chosenBodyIndex;
    private string clickedAnimationName, clickedNewBody;
    private string folderPath;
    private string lastAnimation;

    void Awake()
    {
        InitializeOnGameObjects();
        UpdateInformation();
        LoadObjects();
    }

    void InitializeOnGameObjects()
    {
        actionListPanel.SetActive(false);
        subOptionPanel.SetActive(false);
        SetMessageBoxActive(false);
        SetWarningActive(false);
        SetSavePanelActive(false);
        SetLoadPanelActive(false);
        editorModeActionTypePanel.SetActive(subOptionPanel.activeSelf);
        previewModeActionTypePanel.SetActive(subOptionPanel.activeSelf);
        oldClickedButton = null;
        isModelChosen = false;
        isPreview = false;
        letPlay = false;
        currentCharacter = new Character();
        folderPath = GetFolderPath();
    }

    public void SetSubOptionPanelActive(int subOptionButtonIndex)
    {
        Enums.SubOptionButtons clickedButtonName = (Enums.SubOptionButtons)subOptionButtonIndex;
        Button clickedButton = EditorButtonClicked(clickedButtonName);
        if (subOptionPanel.activeSelf == true && clickedButton == oldClickedButton)
        {     
            SetEditorModeButtons(false, oldClickedButton, oldClickedButtonName.ToString());
            oldClickedButton = null;
        }
        else
        {
            if(subOptionPanel.activeSelf==true)
            {
                SetEditorModeButtons(false, oldClickedButton, oldClickedButtonName.ToString());
            }
            subOptionTitle.text = clickedButtonName + Constants.subOption;
            if(clickedButtonName == Enums.SubOptionButtons.Body || IsBodyChosen(Constants.meesageforBodyIsNotChosen))
            {
                ClearSubOptionPanel();
                SetEditorModeButtons(true, clickedButton, clickedButtonName + Constants.clickToClose);
                oldClickedButton = clickedButton;
                oldClickedButtonName = clickedButtonName;
                switch (clickedButtonName)
                {
                    case Enums.SubOptionButtons.Body:
                        DisplayBodyList();
                        break;

                    case Enums.SubOptionButtons.Texture:
                        DisplayTextureList();
                        break;

                    case Enums.SubOptionButtons.Action:
                        DisplayAnimationList();
                        break;
                }
            }            
        }
    }

    Button EditorButtonClicked(Enums.SubOptionButtons buttonName)
    {
        switch (buttonName)
        {
            case Enums.SubOptionButtons.Body: return btnBody;
            case Enums.SubOptionButtons.Texture: return btnTexture;
            default: return btnAction;
        }
    }

    public void ChangeMode()
    {
        if (IsBodyChosen(Constants.meesageforBodyIsNotChosen))
        {
            isPreview = !isPreview;
            letPlay = false;
            actionListPanel.SetActive(isPreview);
            SetEditorModeButtons(false, oldClickedButton, oldClickedButtonName.ToString());
            txtPreviewButton.text = (isPreview) ? Constants.editorMode : Constants.previewMode;
            if (isPreview)
            {
                ClearActionListPanel();
                actionPanelActionButtonImage.color = normalColor;
                for (int i = 0; i < animationList[chosenBodyIndex].Length; i++)
                {
                    CreateActionPanel(animationList[chosenBodyIndex][i].name);                    
                }                    
            }
            else
            {
                clickedAnimationName = currentCharacter.Action_idle;
                ActionIdleClicked();
            }
        }
    }

    public void SetMessageBoxActive(bool toggle)
    {
        messagePanel.SetActive(toggle);
        blankPanel.SetActive(toggle);
    }

    public void SetWarningActive(bool toggle)
    {
        warningPanel.SetActive(toggle);
        blankPanel.SetActive(toggle);
    }

    public void SetSavePanelActive(bool toggle)
    {
        if(toggle)
            savePanel.GetComponentInChildren<InputField>().text = null;
        savePanel.SetActive(toggle);
        blankPanel.SetActive(toggle);
    }

    public void SetLoadPanelActive(bool toggle)
    {
        loadPanel.SetActive(toggle);
        blankPanel.SetActive(toggle);
    }

    public void OverwriteNewBody()
    {
        lastChosenBody.GetComponentInChildren<Renderer>().material.mainTexture = null;
        currentCharacter.FileName = Constants.none;
        currentCharacter.Texture = Constants.none;
        currentCharacter.Action_idle = Constants.none;
        currentCharacter.Action_run = Constants.none;
        currentCharacter.Action_attack = Constants.none;
        LoadBody(clickedNewBody);
    }

    void UpdateInformation()
    {      
        txtInformation.text = Constants.fileName+currentCharacter.FileName+
                              Constants.body + currentCharacter.Body +
                              Constants.texture + currentCharacter.Texture +
                              Constants.action +
                              Constants.idle + currentCharacter.Action_idle +
                              Constants.run + currentCharacter.Action_run +
                              Constants.attack + currentCharacter.Action_attack;
    }

    void SetEditorModeButtons(bool toggle,Button button, string buttonName)
    {
        editorModeActionTypePanel.SetActive(false);
        previewModeActionTypePanel.SetActive(false);
        subOptionPanel.SetActive(toggle);
        if (button != null)
        {
            button.GetComponentInChildren<Text>().text = buttonName.ToString();
            button.GetComponentInChildren<Image>().color = (toggle) ? clickedColor : normalColor;
        }
    }

    bool IsBodyChosen(string messageText)
    {
        if (isModelChosen)
        {
            return true;
        }
        else
        {
            txtMessage.text = Constants.meesageforBodyIsNotChosen;
            SetMessageBoxActive(true);
            return false;
        }
    }

    void DisplayBodyList()
    {
        for(int i = 0; i < bodyList.Length; i++)
        {
            GameObject newButton = GetSubOptionButton(bodyList[i].name);
            newButton.GetComponent<Button>().onClick.AddListener(delegate { BodySubOptionButtonClicked(newButton.name); });
        }
    }

    void BodySubOptionButtonClicked(string btnName)
    {
        if (isModelChosen)
        {
            clickedNewBody = btnName;
            SetWarningActive(true);            
        }
        else
        {
            LoadBody(btnName);
        }        
    }

    void LoadBody(string bodyName)
    {
        if (lastChosenBody != null)
            lastChosenBody.SetActive(false);
        isModelChosen = true;
        for (int i = 0; i < bodyList.Length; i++)
        {
            if (bodyList[i].name == bodyName)
            {
                bodyList[i].GetComponent<Animation>().clip = null;
                bodyList[i].SetActive(true);                
                lastChosenBody = bodyList[i];
                chosenBodyIndex = i;
                currentCharacter.Body = GetRealName(lastChosenBody.name);
                UpdateInformation();
                break;
            }
        }
    }

    void DisplayTextureList()
    {
        for (int i = 0; i < textureList.Length; i++)
        {
            GameObject newButton = GetSubOptionButton(textureList[i].name);
            newButton.GetComponent<Button>().onClick.AddListener(delegate { TextureSubOptionButtonClicked(newButton.name); });
        }
    }

    void TextureSubOptionButtonClicked(string btnName)
    {
        for (int i = 0; i < textureList.Length; i++)
        {
            if (textureList[i].name == btnName)
            {
                lastChosenBody.GetComponentInChildren<Renderer>().material.mainTexture = textureList[i];
                currentCharacter.Texture = GetRealName(textureList[i].name);
                UpdateInformation();
                break;
            }
        }
    }

    void DisplayAnimationList()
    {
        for(int i = 0; i < animationList[chosenBodyIndex].Length; i++)
        {
            GameObject newButton = GetSubOptionButton(animationList[chosenBodyIndex][i].name);
            newButton.GetComponent<Button>().onClick.AddListener(delegate { AnimationSubOptionButtonClicked(newButton.name); });
        }
    }

    void AnimationSubOptionButtonClicked(string btnName)
    {
        editorModeActionTypePanel.SetActive(true);
        clickedAnimationName = btnName;
    }

    public void ActionIdleClicked()
    {
        editorModeActionTypePanel.SetActive(false);
        currentCharacter.Action_idle = clickedAnimationName;
        lastChosenBody.GetComponent<Animation>().Play(clickedAnimationName);
        lastChosenBody.GetComponent<Animation>().cullingType = AnimationCullingType.AlwaysAnimate;
        UpdateInformation();
    }

    public void ActionRunClicked()
    {
        editorModeActionTypePanel.SetActive(false);
        currentCharacter.Action_run = clickedAnimationName;
        UpdateInformation();
    }

    public void ActionAttackClicked()
    {
        editorModeActionTypePanel.SetActive(false);
        currentCharacter.Action_attack = clickedAnimationName;
        UpdateInformation();
    }

    void LoadObjects()
    {
        //Body & Animations
        bodyList = Resources.LoadAll<GameObject>(Constants.modelFolderPath);
        animationList = new AnimationClip[bodyList.Length][];
        for (int i = 0; i < bodyList.Length; i++)
        {
            animationList[i] = Resources.LoadAll<AnimationClip>(Path.Combine(Constants.modelFolderPath, bodyList[i].name));
        }
        for (int i = 0; i < bodyList.Length; i++)
        {
            bodyList[i] = Instantiate(bodyList[i], new Vector3(0, 0, 0), Quaternion.identity);
            bodyList[i].SetActive(false);
        }

        //Texture
        textureList =Resources.LoadAll<Texture>(Constants.textureFolderPath);
    }

    void ClearSubOptionPanel()
    {
        foreach (Transform child in subOptionContent)
            GameObject.Destroy(child.gameObject);
    }
    
    void ClearActionListPanel()
    {
        foreach (Transform child in actionListContent)
            GameObject.Destroy(child.gameObject);
    }

    string GetRealName(string text)
    {
        return (text.Replace(Constants.extraNameMs_, string.Empty)).Replace(Constants.extraNameClone,string.Empty);
    }

    GameObject GetSubOptionButton(string btnName)
    {
        GameObject newButton = Instantiate(subOptionButtonPrefab);
        newButton.name = btnName;
        newButton.GetComponentInChildren<Text>().text = GetRealName(btnName);
        newButton.transform.SetParent(subOptionContent, false);
        return newButton;
    }

    void CreateActionPanel(string btnName)
    {
        GameObject newActionPanel = Instantiate(actionPanelPrefab);
        newActionPanel.transform.SetParent(actionListContent, false);
        Button[] buttons = newActionPanel.GetComponentsInChildren<Button>();
        Button actionButton, checkButton;
        for(int i=0;i<buttons.Length;i++)
        {
            if (buttons[i].name == Constants.checkButtonName)
            {
                checkButton = buttons[i];
                checkButton.onClick.AddListener(delegate { ActionPanelCheckButtonClicked(btnName); });
            }
            else
            {
                actionButton = buttons[i];
                actionButton.GetComponentInChildren<Text>().text = btnName;
                actionButton.onClick.AddListener(delegate { ActionPanelActionButtonClicked(btnName); });
            }                          
        }       
    }

    void ActionPanelCheckButtonClicked(string btnName)
    {
        previewModeActionTypePanel.SetActive(false);
        lastChosenBody.GetComponent<Animation>().Play(btnName);
    }

    void ActionPanelActionButtonClicked(string btnName)
    {
        previewModeActionTypePanel.SetActive(true);
        clickedAnimationName = btnName;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (lastChosenBody!=null && currentCharacter.Action_idle!=Constants.none && !lastChosenBody.GetComponent<Animation>().isPlaying)
        {
            if (letPlay)
            {
                if (lastAnimation == currentCharacter.Action_idle)
                {
                    lastChosenBody.GetComponent<Animation>().Play(currentCharacter.Action_run);
                    lastAnimation = currentCharacter.Action_run;
                }

                else if (lastAnimation == currentCharacter.Action_run)
                {
                    lastChosenBody.GetComponent<Animation>().Play(currentCharacter.Action_attack);
                    lastAnimation = currentCharacter.Action_attack;
                }

                else
                {
                    lastChosenBody.GetComponent<Animation>().Play(currentCharacter.Action_idle);
                    lastAnimation = currentCharacter.Action_idle;
                }
            }
            else
            {
                lastChosenBody.GetComponent<Animation>().Play(currentCharacter.Action_idle);
            }            
        }
    }

    public void SaveButtonClicked()
    {
        if (IsBodyChosen(Constants.messageForSaveButtonClickedWithoutChoosingBody))
        {
            SetSavePanelActive(true);
        }
    }

    string GetFolderPath()
    {
        string path = Path.Combine(Application.persistentDataPath, Constants.folderName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    public void SaveConfirmed()
    {
        string characterName=savePanel.GetComponentInChildren<InputField>().text;
        if (characterName != string.Empty)
        {
            currentCharacter.FileName = characterName;
            string dataPath = Path.Combine(folderPath, characterName + Constants.fileExtension);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using(FileStream fileStream = File.Open(dataPath, FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fileStream, currentCharacter);
            }
            UpdateInformation();
        }
        else
        {
            txtMessage.text = Constants.messageForCharacterNameIsNull;
            SetMessageBoxActive(true);
        }
    }

    public void LoadButtonClicked()
    {
        List<string> savedCharacterNames = GetSavedCharacterNames();
        if (savedCharacterNames.Count > 0)
        {
            SetLoadPanelActive(true);
            loadPanel.GetComponentInChildren<Dropdown>().ClearOptions();
            loadPanel.GetComponentInChildren<Dropdown>().AddOptions(savedCharacterNames);
        }
        else
        {
            txtMessage.text = Constants.messageForNoCharacterIsSavedInFolder;
            SetMessageBoxActive(true);
        }
    }

    List<string> GetSavedCharacterNames()
    {
        string[] files = Directory.GetFiles(folderPath, Constants.all + Constants.fileExtension);
        List<string> filesList = new List<string>();
        for (int i = 0; i < files.Length; i++)
            filesList.Add(Path.GetFileNameWithoutExtension(files[i]));
        return filesList;
    }

    public void LoadConfirmed()
    {
        Dropdown loadPanelDropDown = loadPanel.GetComponentInChildren<Dropdown>();
        string chosenCharacterName = loadPanelDropDown.options[loadPanelDropDown.value].text;
        Character chosenCharacter = GetSavedCharacter(chosenCharacterName);
        LoadCharacter(chosenCharacter);
    }

    Character GetSavedCharacter(string characterName)
    {
        string dataPath = Path.Combine(folderPath, characterName + Constants.fileExtension);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (FileStream fileStream = File.Open(dataPath, FileMode.Open))
        {
            return (Character)binaryFormatter.Deserialize(fileStream);
        }
    }

    void LoadCharacter(Character character)
    {
        currentCharacter = character;
        UpdateInformation();
        LoadBody(Constants.extraNameMs_+currentCharacter.Body+Constants.extraNameClone);
        TextureSubOptionButtonClicked(Constants.extraNameMs_+currentCharacter.Texture);
        clickedAnimationName = currentCharacter.Action_idle;
        ActionIdleClicked();
    }

    public void ActionListPanelPlayActionButtonClicked()
    {
        letPlay = !letPlay;
        actionPanelActionButtonImage.color = (letPlay) ? clickedColor : normalColor;
        lastAnimation = string.Empty;
    }
}