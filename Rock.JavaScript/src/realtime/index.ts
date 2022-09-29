import { AspNetEngine } from "./aspNetEngine";
import { Topic } from "./topic";
import { GenericServerFunctions, ServerFunctions } from "./types";

const engine = new AspNetEngine();

async function getTopic<TServer extends ServerFunctions<TServer> = GenericServerFunctions>(identifier: string): Promise<Topic<TServer>> {
    await engine.ensureConnected();

    const topic = new Topic<TServer>(identifier, engine);

    await topic.connect();

    return topic;
}

interface IServer {
    ping(text: string, value: number): Promise<void>;
}

(async function () {
    const topic = await getTopic<IServer>("Rock.RealTime.Topics.TestTopic");

    topic.on("Pong", (value: number) => console.log("Received pong", value));
    topic.server.ping("This is some test", 100);
})();

export { getTopic };
