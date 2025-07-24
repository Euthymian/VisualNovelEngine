using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class ConversationQueue
    {
        private Queue<Conversation> conversationQueue = new Queue<Conversation>();
        
        public Conversation top => conversationQueue.Peek();
        
        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);

        public void EnqueuePriority(Conversation conversation)
        {
            Queue<Conversation> tempQueue = new Queue<Conversation>();
            tempQueue.Enqueue(conversation);

            while (conversationQueue.Count > 0)
                tempQueue.Enqueue(conversationQueue.Dequeue());

            conversationQueue = tempQueue;
        }

        public void Dequeue()
        {
            if (conversationQueue.Count > 0)
                conversationQueue.Dequeue();
        }

        public void Clear() => conversationQueue.Clear();

        public bool IsEmpty() => conversationQueue.Count == 0;

        public Conversation[] GetConversationQueue() => conversationQueue.ToArray();
    }
}