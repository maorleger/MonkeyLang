/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
// tslint:disable
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const vscode_1 = require("vscode");
const vscode_languageclient_1 = require("vscode-languageclient");
const vscode_jsonrpc_1 = require("vscode-jsonrpc");
function activate(context) {
    // The server is implemented in node
    let serverExe = 'dotnet';
    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    let serverOptions = {
        run: { command: serverExe, args: ['C:\workspace\MonkeyLang\MonkeyLang.LanguageServer\bin\Debug\netcoreapp3.1\MonkeyLang.LanguageServer.dll'] },
        debug: { command: serverExe, args: ['C:\workspace\MonkeyLang\MonkeyLang.LanguageServer\bin\Debug\netcoreapp3.1\MonkeyLang.LanguageServer.dll'] }
    };
    // Options to control the language client
    let clientOptions = {
        documentSelector: [
            {
                pattern: '**/*.monkey',
            }
        ],
        synchronize: {
            // Synchronize the setting section 'languageServerExample' to the server
            configurationSection: 'languageServerExample',
            fileEvents: vscode_1.workspace.createFileSystemWatcher('**/*.monkey')
        },
    };
    // Create the language client and start the client.
    const client = new vscode_languageclient_1.LanguageClient('monkeyLangLSP', 'Language Server for the Monkey Programming Language', serverOptions, clientOptions);
    client.trace = vscode_jsonrpc_1.Trace.Verbose;
    let disposable = client.start();
    // Push the disposable to the context's subscriptions so that the
    // client can be deactivated on extension deactivation
    context.subscriptions.push(disposable);
}
exports.activate = activate;
//# sourceMappingURL=extension.js.map