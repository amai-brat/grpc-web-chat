import { AuthServiceClient } from '../protos_gen/auth.client';
import {useState} from "react";
import { transport } from "../App";

interface JoinProps {
    setIsJoined: (isJoined: boolean) => void;
}

export const Join = ({setIsJoined}: JoinProps) => {
    const [name, setName] = useState<string>("");
    
    async function handleJoinClick() {
        const authClient = new AuthServiceClient(transport);
        const tokenCall = await authClient.getToken({
            name: name
        });
        console.log(tokenCall);
        
        if (tokenCall.status.code == "OK") {
            sessionStorage.setItem('token', tokenCall.response.jwtToken);
            setIsJoined(true);
            return;
        }
    }

    return (
        <div style={{display: 'flex', flexDirection: 'column'}}>
            <label htmlFor={"name"}>Имя: </label>
            <input id={"name"} type={"text"} value={name} onChange={e => setName(e.target.value)}/>
            <button type={"button"} onClick={handleJoinClick}>Join</button>
        </div>
    )
}