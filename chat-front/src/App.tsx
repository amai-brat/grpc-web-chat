import './App.css'
import {GrpcWebFetchTransport} from "@protobuf-ts/grpcweb-transport";
import {useState} from "react";
import { Join } from './components/Join';
import { Chat } from './components/Chat';
import {MethodInfo, NextUnaryFn, RpcOptions, UnaryCall } from '@protobuf-ts/runtime-rpc';

export const transport = new GrpcWebFetchTransport({
  baseUrl: 'http://localhost:8080',
  interceptors: [
    { 
      interceptUnary(next: NextUnaryFn, method: MethodInfo, input: object, options: RpcOptions): UnaryCall {
        if (!options.meta) {
          options.meta = {};
        }
        
        const token = sessionStorage.getItem('token');
        if (token) {
          options.meta['Authorization'] = `Bearer ${token}`;
        }
        
        return next(method, input, options);
      }
    }
  ]
});

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
