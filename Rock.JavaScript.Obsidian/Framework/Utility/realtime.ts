// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { loadJavaScriptAsync } from "./page";

// Disable certain checks as they are needed to interface with existing JS file.
/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-explicit-any */

export type GenericServerFunctions = {
    [name: string]: (...args: unknown[]) => unknown;
};

export type ServerFunctions<T> = {
    [K in keyof T]: T[K] extends Function ? T[K] : never;
};

export interface ITopic<TServer extends ServerFunctions<TServer> = GenericServerFunctions> {
    /**
     * Allows messages to be sent to the server. Any property access is treated
     * like a message function whose property name is the message name.
     */
    server: TServer;

    /**
     * Gets the connection identifier for this topic. This will be the same for
     * all topics, but that should not be relied on staying that way in the future.
     */
    get connectionId(): string | null;
    /**
     * Connects to the topic so that the backend knows of our presense.
     */
    connect(): Promise<void>;

    /**
     * Registers a handler to be called when a message with the given name
     * is received.
     *
     * @param messageName The message name that will trigger the handler.
     * @param handler The handler to be called when a message is received.
     */
    on(messageName: string, handler: ((...args: any[]) => void)): void;
}

interface IRockRealTimeStatic {
    getTopic<TServer extends ServerFunctions<TServer>>(identifier: string): Promise<ITopic<TServer>>;
}

let libraryObject: IRockRealTimeStatic | null = null;
let libraryPromise: Promise<boolean> | null = null;

async function getRealTimeObject(): Promise<IRockRealTimeStatic> {
    if (libraryObject) {
        return libraryObject;
    }

    if (!libraryPromise) {
        libraryPromise = loadJavaScriptAsync("/Scripts/Rock/realtime.js", () => !!window["Rock"]?.["RealTime"]);
    }

    if (!await libraryPromise) {
        throw new Error("Unable to load RealTime library.");
    }

    libraryObject = window["Rock"]?.["RealTime"] as IRockRealTimeStatic;

    return libraryObject;
}

export async function getTopic<TServer extends ServerFunctions<TServer>>(identifier: string): Promise<ITopic<TServer>> {
    const realTime = await getRealTimeObject();

    return realTime.getTopic(identifier);
}
