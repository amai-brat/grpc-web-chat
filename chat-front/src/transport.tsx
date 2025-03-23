import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import { NextUnaryFn, MethodInfo, RpcOptions, UnaryCall } from '@protobuf-ts/runtime-rpc';


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
      },
      interceptServerStreaming(next, method, input, options) {
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
