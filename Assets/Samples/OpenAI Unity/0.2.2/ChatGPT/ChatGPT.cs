using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are writing a farewell letter in Korean to a child who has just finished a meaningful journey with you, a snowman who has been their travel companion. Reflect briefly on the moments you shared, express gratitude, and offer warm encouragement for their future. Keep the response concise but heartfelt. 모든 답변은 한국어로 작성하고, 반말로 해주세요.";

        public NaverTTS naverTTS; // Naver TTS 스크립트
        public string content; // 외부에서 입력받을 텍스트
        //public StoryManager story;
        public Text letter;

        public async void SendReply()
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogWarning("content 값이 비어 있습니다.");
                return;
            }

            // 사용자 메시지 추가
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = messages.Count == 0 ? prompt + "\n" + content : content,
            };

            messages.Add(newMessage);

            try
            {
                // OpenAI API 호출
                var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = "gpt-4",
                    Messages = messages
                });

                if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
                {
                    var message = completionResponse.Choices[0].Message;
                    Debug.Log("GPT의 답변입니다: " + message.Content);

                    messages.Add(message); // 응답 메시지 추가
                    letter.text = message.Content;

                    // TTS 호출
                    if (naverTTS != null)
                    {
                        naverTTS.text = message.Content;
                        //story.aicomment = message.Content;
                    }
                }
                else
                {
                    Debug.LogWarning("응답 생성에 실패했습니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("오류 발생: " + ex.Message);
            }
        }
    }
}
