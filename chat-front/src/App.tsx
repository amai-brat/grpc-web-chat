import './App.css'
import {useState} from "react";
import { Join } from './components/Join';
import { Chat } from './components/Chat';

function App() {
  const [isJoined, setIsJoined] = useState<boolean>(false);
  
  return (
    <>
      {!isJoined && <Join setIsJoined={setIsJoined} />}
      {isJoined && <Chat/>}
    </>
  )
}

export default App
