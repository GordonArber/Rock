import { hubConnection, Proxy } from "signalr-no-jquery";
import mitt from "mitt";

type Topic<TServer> = {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    on(messageName: string, handler: ((...args: any[]) => void)): void;

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    server: TServer;
};

const emitter = mitt();
let hubProxy: Proxy | null = null;
let isStarting: boolean = false;
let startResolve: () => void;
let startReject: (reason: unknown) => void;

const startPromise = new Promise<void>((resolve, reject) => {
    startResolve = resolve;
    startReject = reject;
});

async function startConnectionAsync(): Promise<Proxy> {
    return new Promise<Proxy>((resolve, reject) => {
        const connection = hubConnection("/rock-rt", { useDefaultPath: false });
        const hub = connection.createHubProxy("realTime");

        hub.on("message", (topicName: string, messageName: string, messageParams: unknown[]) => {
            emitter.emit(`${topicName}-${messageName}`, messageParams);
        });

        connection.start()
            .done(() => {
                resolve(hub);
            })
            .fail(() => {
                reject(new Error("Failed to connect to RealTime hub."));
            });
    });
}

async function start(): Promise<void> {
    if (isStarting) {
        return await startPromise;
    }

    isStarting = true;

    try {
        hubProxy = await startConnectionAsync();
        startResolve();
    }
    catch (error: unknown) {
        startReject(error);
    }

    if (hubProxy) {
        await hubProxy.invoke("postMessage", "Rock.RealTime.Topics.TestTopic", "Ping", ["hello", 42]);
    }
}

async function invoke(topicName: string, messageName: string, messageParams: unknown[]): Promise<any> {
    if (!hubProxy) {
        await startPromise;
    }

    // This is really just to make TypeScript happy, but it's a good final
    // safety check.
    if (!hubProxy) {
        return;
    }

    await hubProxy.invoke("postMessage", topicName, messageName, messageParams);
}

start();

type GenericServerFunctions = {
    [name: string]: (...args: any[]) => any;
};

// eslint-disable-next-line @typescript-eslint/ban-types
function getTopic<TServer extends { [K in keyof TServer]: TServer[K] extends Function ? TServer[K] : never } = GenericServerFunctions>(identifier: string): Topic<TServer> {
    if (!hubProxy && !isStarting) {
        start();
    }

    const serverProxy = new Proxy<TServer>({} as TServer, {
        get(_, propertyName) {
            return async (...args: unknown[]): Promise<unknown> => {
                if (typeof propertyName !== "string") {
                    return;
                }

                return await invoke(identifier, propertyName, args);
            };
        }
    });

    return {
        on(messageName, handler) {
            emitter.on(`${identifier}-${messageName}`, (args: unknown) => {
                handler(...(args as unknown[]));
            });
        },

        server: serverProxy
    };
}

interface IServer {
    ping(text: string, value: number): Promise<void>;
}

const topic = getTopic<IServer>("Rock.RealTime.Topics.TestTopic");

topic.on("Pong", (value: number) => console.log("Received pong", value));
topic.server.ping("This is some test", 100);

const topic2 = getTopic("Rock.RealTime.Topics.TestTopic");
topic2.server.blah(1, 2, 3);

export { getTopic };

