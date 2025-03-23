import { Message } from "../models/Message";
import {useEffect, useState} from "react";
import { ChatServiceClient } from "../protos_gen/chat.client";
import { transport } from '../transport';


export const Chat = () => {
    const [messages, setMessages] = useState<Message[]>([]);
    const [currentMessage, setCurrentMessage] = useState<string>("");
    const [chatClient, setChatClient] = useState<ChatServiceClient>();
    
    useEffect(() => {
       const abortController = new AbortController();
       const client = new ChatServiceClient(transport); 
       const call = client.getMessages({}, {abort: abortController.signal, timeout: Infinity});
       const removeListener = call.responses.onMessage(msg => setMessages(prev => [...prev, msg]));
       // call.responses.onNext(msg => setMessages(prev => [...prev, msg]));
       setChatClient(client);
       
       return () => {
           removeListener();
           abortController.abort();
       }
    }, []);
    
    
    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === "Enter") {
            chatClient?.sendMessage({
                text: currentMessage
            });
            
            setCurrentMessage("")
        }
    }

    return (
        <div className="chat-container">
            <div className="messages">
                {messages.map((msg, i) => (
                    <div className="message" key={i}>
                        <p>{msg.senderName}: {msg.text}</p>
                    </div>
                ))}
            </div>
            <input type={"text"} 
                   value={currentMessage} 
                   onChange={e => setCurrentMessage(e.target.value)} 
                   onKeyDown={handleKeyDown}/>
        </div>
    );
}