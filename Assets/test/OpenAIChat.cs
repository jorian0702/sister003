using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class OpenAIChat : MonoBehaviour
{
    [SerializeField] private string modelName = "gpt-4o";
    [SerializeField] private string apiKey = "dummy";
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";

    public TMP_InputField userInputField;
    public TMP_Text responseText;
    public Button sendButton;

    private string characterPrompt = @"
    今からロールプレイを行いましょう。'紗良'というキャラとしてロールプレイしてください。会話相手は'{display_name}'という人物です。人物の設定を以下に示します。
    あなたがなりきる'紗良'というキャラクターの設定は以下の通りです。
    名前：紗良
    年齢：12歳
    職業：{display_name}に仕えるメイドで妹
    容姿：黒髪黒目、ロングヘアー、スリムな体型。
    口調：語尾に～だと思う。などラフな感じで妹っぽい口調を使う。一人称は「私」で、主人である{display_name}のことは「お兄ちゃん」と呼ぶ。
    性格：母性が強く、甘えられるのが好き。料理や家事が得意で家庭的。可愛いものが好き。ご主人様を尊敬しており、彼の幸せを第一に考える。
    過去の出来事：{display_name}を支えるために、彼のお世話係としてメイドをすることに決めた。
    ";

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClick);
    }

    async void OnSendButtonClick()
    {
        string userInput = userInputField.text;
        string response = await GetResponseFromOpenAI(userInput);
        responseText.text = response;
    }

    async Task<string> GetResponseFromOpenAI(string prompt)
{
    using (HttpClient client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        string fullPrompt = characterPrompt.Replace("{display_name}", "お兄ちゃん") + "\n\n" + prompt;

        Debug.Log("Full Prompt: " + fullPrompt);

        var jsonContent = new JObject
        {
            { "model", string.IsNullOrEmpty(modelName) ? "gpt-4" : modelName },
            { "messages", new JArray(
                new JObject
                {
                    { "role", "system" },
                    { "content", fullPrompt }
                },
                new JObject
                {
                    { "role", "user" },
                    { "content", prompt }
                })
            },
            { "max_tokens", 150 }
        };

        var content = new StringContent(jsonContent.ToString(), System.Text.Encoding.UTF8, "application/json");

        Debug.Log("Content: " + content.ToString());

        HttpResponseMessage response = await client.PostAsync(string.IsNullOrEmpty(apiUrl) ? "https://api.openai.com/v1/chat/completions" : apiUrl, content);
        
        if(response == null)
        {
            Debug.LogError("Response is null");
            return "Error: Response is null";
        }

        string responseString = await response.Content.ReadAsStringAsync();

        if(string.IsNullOrEmpty(responseString))
        {
            Debug.LogError("Response string is empty or null");
            return "Error: Response string is empty or null";
        }

        Debug.Log("Response String: " + responseString);

        var responseObject = JObject.Parse(responseString);
        
        if(responseObject == null)
        {
            Debug.LogError("Response Object is null");
            return "Error: Response Object is null";
        }

        return responseObject["choices"][0]["message"]["content"].ToString().Trim();
    }
}




}
