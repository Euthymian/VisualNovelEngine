using DIALOGUE;
using HISTORY;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class VNGameSave
    {
        public static VNGameSave activeFile = null;

        public const string FILE_TYPE = ".vns";
        public const string SCREENSHOT_TYPE = ".jpg";
        public const bool ENCRYPT = true;

        public string filePath => $"{FilePaths.gameSaves}{slotNumber}{FILE_TYPE}";
        public string screenshotPath => $"{FilePaths.gameSaves}{slotNumber}{SCREENSHOT_TYPE}";

        public int slotNumber = 1;

        public string playerName;

        // Use Json for conversations is becuase we dont know what kind of conversation we will have
        public string[] activeConversations;
        public HistoryState activeState;
        public HistoryState[] historyLogs;
        public VN_VariableData[] variableData;

        public void Save()
        {
            activeState = HistoryState.Capture();
            historyLogs = HistoryManager.Instance.historyStateList.ToArray();
            activeConversations = GetConversationData();
            variableData = GetVariableData();

            string saveJSON = JsonUtility.ToJson(this);

            FileManager.Save(filePath, saveJSON, ENCRYPT);
        }

        public static VNGameSave Load(string filePath, bool activateOnLoad = false)
        {
            VNGameSave save = FileManager.Load<VNGameSave>(filePath, ENCRYPT);
            activeFile = save;

            if(activateOnLoad)
                save.Activate();
        
            return save;
        }

        public void Activate()
        {
            if(activeState != null)
                activeState.Load();

            HistoryManager.Instance.historyStateList = historyLogs.ToList();
            HistoryManager.Instance.historyLogManager.ClearLogs();
            HistoryManager.Instance.historyLogManager.Rebuild();

            DialogueSystem.Instance.dialogueContinuePrompt.Hide(); //Hide the continue prompt so it waits for new text is finished building
            ChoicePanel.Instance.Hide();
            InputPanel.Instance.Hide();

            SetConversationData();
            SetVaribleData();
        }

        private string[] GetConversationData()
        {
            List<string> res = new List<string>();

            Conversation[] conversations = DialogueSystem.Instance.conversationManager.GetConversationQueue();

            for(int i = 0; i< conversations.Length; i++)
            {
                Conversation conversation = conversations[i];
                string data = "";

                if(conversation.file != string.Empty)
                {
                    VN_ConversationDataCompressed compressedData = new VN_ConversationDataCompressed();
                    compressedData.fileName = conversation.file;
                    compressedData.progress = conversation.GetProgress();
                    compressedData.startIndex = conversation.fileStartIndex;
                    compressedData.endIndex = conversation.fileEndIndex;
                    data = JsonUtility.ToJson(compressedData);
                }
                else
                {
                    VN_ConversationData fullData = new VN_ConversationData();
                    fullData.conversation = conversation.GetLines();
                    fullData.progress = conversation.GetProgress();
                    data = JsonUtility.ToJson(fullData);
                }

                res.Add(data);
            }

            return res.ToArray();
        }

        private void SetConversationData()
        {
            for (int i = 0; i < activeConversations.Length; i++)
            {
                try
                {
                    string data = activeConversations[i];
                    Conversation conversation = null;

                    var fullData = JsonUtility.FromJson<VN_ConversationData>(data);
                    if (fullData != null && fullData.conversation != null && fullData.conversation.Count > 0)
                    {
                        conversation = new Conversation(fullData.conversation, fullData.progress);
                    }
                    else
                    {
                        var compressedData = JsonUtility.FromJson<VN_ConversationDataCompressed>(data);
                        if (compressedData != null && compressedData.fileName != string.Empty)
                        {
                            TextAsset file = Resources.Load<TextAsset>(compressedData.fileName);

                            int count = compressedData.endIndex - compressedData.startIndex + 1;
                            List<string> lines = FileManager.ReadTextAsset(compressedData.fileName).Skip(compressedData.startIndex).Take(count).ToList();

                            conversation = new Conversation(lines, compressedData.progress, compressedData.fileName, compressedData.startIndex, compressedData.endIndex);
                        }
                        else
                        {
                            Debug.LogError($"Unknown conversation data format: {data}");
                        }
                    }

                    if (conversation != null && conversation.GetLines().Count > 0)
                    {
                        if(i==0)
                            DialogueSystem.Instance.conversationManager.StartConversation(conversation);
                        else
                            DialogueSystem.Instance.conversationManager.Enqueue(conversation);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error while parsing conversation data at index {i}: {e}");
                    continue;
                }
            }
        }

        private VN_VariableData[] GetVariableData()
        {
            List<VN_VariableData> res = new List<VN_VariableData>();

            foreach (var database in VariableStore.databases.Values)
            {
                foreach(var variable in database.variables)
                {
                    VN_VariableData data = new VN_VariableData();
                    data.name = $"{database.name}.{variable.Key}";
                    string val = $"{variable.Value.Get()}";
                    data.value = val;
                    data.type = val == string.Empty ? "System.String" : variable.Value.Get().GetType().ToString();
                    res.Add(data);
                }
            }

            return res.ToArray();
        }

        private void SetVaribleData()
        {
            foreach(var variable in variableData)
            {
                string val = variable.value;

                switch (variable.type)
                {
                    case "System.Boolean":
                        if(bool.TryParse(val, out bool boolValue))
                        {
                            VariableStore.TrySetValue(variable.name, boolValue);
                            continue;
                        }
                        break;
                    case "System.Int32":
                        if (int.TryParse(val, out int intValue))
                        {
                            VariableStore.TrySetValue(variable.name, intValue);
                            continue;
                        }
                        break;
                    case "System.Single":
                        if (float.TryParse(val, out float floatValue))
                        {
                            VariableStore.TrySetValue(variable.name, floatValue);
                            continue;
                        }
                        break;
                    case "System.Double":
                        if (double.TryParse(val, out double doubleValue))
                        {
                            VariableStore.TrySetValue(variable.name, doubleValue);
                            continue;
                        }
                        break;
                    case "System.String":
                        VariableStore.TrySetValue(variable.name, val);
                        continue;
                }

                Debug.LogError($"Could not interpret variable {variable.name} with type {variable.type}");
            }
        }
    }
}