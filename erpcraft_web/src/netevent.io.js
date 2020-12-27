"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function () { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function () { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var PacketType;
(function (PacketType) {
    PacketType[PacketType["S_initOk"] = 0] = "S_initOk";
    PacketType[PacketType["C_init"] = 1] = "C_init";
    PacketType[PacketType["S_initErr"] = 2] = "S_initErr";
    PacketType[PacketType["C_authSend"] = 3] = "C_authSend";
    PacketType[PacketType["S_authOk"] = 4] = "S_authOk";
    PacketType[PacketType["C_sendEvent"] = 5] = "C_sendEvent";
    PacketType[PacketType["S_authErr"] = 6] = "S_authErr";
    PacketType[PacketType["C_sendEventForCallback"] = 7] = "C_sendEventForCallback";
    PacketType[PacketType["S_sendEvent"] = 8] = "S_sendEvent";
    PacketType[PacketType["C_sendCallbackForEvent"] = 9] = "C_sendCallbackForEvent";
    PacketType[PacketType["S_sendEventForCallback"] = 10] = "S_sendEventForCallback";
    PacketType[PacketType["C_subscribe"] = 11] = "C_subscribe";
    PacketType[PacketType["S_sendCallbackForEvent"] = 12] = "S_sendCallbackForEvent";
    PacketType[PacketType["C_unsubscribe"] = 13] = "C_unsubscribe";
    PacketType[PacketType["S_subscriptionPush"] = 14] = "S_subscriptionPush";
    PacketType[PacketType["C_unsubscribeAll"] = 15] = "C_unsubscribeAll";
    PacketType[PacketType["S_binaryEvent"] = 16] = "S_binaryEvent";
    PacketType[PacketType["C_binaryEvent"] = 17] = "C_binaryEvent";
})(PacketType || (PacketType = {}));
exports.PacketType = PacketType;
var NetEventIOClientState;
(function (NetEventIOClientState) {
    NetEventIOClientState[NetEventIOClientState["Connected"] = 0] = "Connected";
    NetEventIOClientState[NetEventIOClientState["Initialized"] = 1] = "Initialized";
    NetEventIOClientState[NetEventIOClientState["Authenticated"] = 2] = "Authenticated";
    NetEventIOClientState[NetEventIOClientState["Disconnected"] = 3] = "Disconnected";
})(NetEventIOClientState || (NetEventIOClientState = {}));
exports.NetEventIOClientState = NetEventIOClientState;
var SubscriptionChangeType;
(function (SubscriptionChangeType) {
    SubscriptionChangeType[SubscriptionChangeType["insert"] = 0] = "insert";
    SubscriptionChangeType[SubscriptionChangeType["update"] = 1] = "update";
    SubscriptionChangeType[SubscriptionChangeType["delete"] = 2] = "delete";
})(SubscriptionChangeType || (SubscriptionChangeType = {}));
exports.SubscriptionChangeType = SubscriptionChangeType;
var Message = /** @class */ (function () {
    function Message(id, packetType, eventName, command, message) {
        this.id = id || this.generateMessageID();
        this.packetType = packetType;
        this.eventName = eventName;
        this.command = command;
        this.message = message;
    }
    Message.prototype.generateMessageID = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };
    return Message;
}());
exports.Message = Message;
var BinaryMessage = /** @class */ (function () {
    function BinaryMessage(id, packetType, eventName, command, message) {
        this.id = id || this.generateMessageID();
        this.packetType = packetType;
        this.eventName = eventName;
        this.command = command;
        this.message = message;
    }
    BinaryMessage.prototype.generateMessageID = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };
    return BinaryMessage;
}());
var NetEventIOOptions = /** @class */ (function () {
    function NetEventIOOptions(onPasswordLogin, onTokenLogin) {
        this.onPasswordLogin = function () {
            return new Promise(function (resolve) {
                resolve("");
            });
        };
        this.onTokenLogin = function () {
            return new Promise(function (resolve) {
                resolve(null);
            });
        };
        this.onPasswordLogin = onPasswordLogin;
        this.onTokenLogin = onTokenLogin;
    }
    return NetEventIOOptions;
}());
exports.NetEventIOOptions = NetEventIOOptions;
var NetEventIO_Client = /** @class */ (function () {
    function NetEventIO_Client(client, options) {
        this.CURRENT_VERSION = 1;
        this.client = client;
        this.state = NetEventIOClientState.Connected;
        this.messageListeners = {};
        this.callbackQueue = {};
        this.subscriptionListeners = {};
        this.onPasswordLogin = options.onPasswordLogin;
        this.onTokenLogin = options.onTokenLogin;
    }
    NetEventIO_Client.prototype.connect = function () {
        var _this = this;
        return new Promise(function (resolve) {
            return __awaiter(_this, void 0, void 0, function () {
                var result;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, new Promise(function (connResolve) {
                            _this.client.onopen = function () {
                                connResolve(true);
                            };
                            _this.client.onclose = function () {
                                connResolve(false);
                            };
                            _this.client.onerror = function () {
                                connResolve(false);
                            };
                        })];
                        case 1:
                            result = _a.sent();
                            if (!result) {
                                resolve([false, 0]);
                                return [2 /*return*/];
                            }
                            return [4 /*yield*/, this.handleInit()];
                        case 2:
                            if (!(_a.sent())) {
                                this.state = NetEventIOClientState.Disconnected;
                                resolve([false, 1]);
                                return [2 /*return*/];
                            }
                            this.state = NetEventIOClientState.Initialized;
                            return [4 /*yield*/, this.handleAuth()];
                        case 3:
                            if (!(_a.sent())) {
                                this.state = NetEventIOClientState.Disconnected;
                                resolve([false, 2]);
                                return [2 /*return*/];
                            }
                            this.state = NetEventIOClientState.Authenticated;
                            this.client.binaryType = "blob";
                            resolve([true]);
                            setTimeout(function () {
                                _this.run();
                            }, 0);
                            return [2 /*return*/];
                    }
                });
            });
        });
    };
    NetEventIO_Client.prototype.run = function () {
        return __awaiter(this, void 0, void 0, function () {
            var o, bm, m;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!(this.client.readyState === this.client.OPEN)) return [3 /*break*/, 5];
                        return [4 /*yield*/, this.getNewMessage()];
                    case 1:
                        o = _a.sent();
                        if (!(o instanceof Blob)) return [3 /*break*/, 3];
                        return [4 /*yield*/, this.parseBinaryMessage(o)];
                    case 2:
                        bm = _a.sent();
                        switch (bm.packetType) {
                            case 16: {
                                this.handleBinaryEvent(bm);
                                break;
                            }
                        }
                        return [3 /*break*/, 0];
                    case 3: return [4 /*yield*/, this.parseMessage(o)];
                    case 4:
                        m = _a.sent();
                        if (m != null) {
                            switch (m.packetType) {
                                case PacketType.S_sendEvent: {
                                    this.handleEvent(m);
                                    break;
                                }
                                case PacketType.S_sendCallbackForEvent: {
                                    this.handleCallbackResponse(m);
                                    break;
                                }
                                case PacketType.S_subscriptionPush: {
                                    this.handleSubscriptionPush(m);
                                    break;
                                }
                            }
                        }
                        return [3 /*break*/, 0];
                    case 5:
                        console.log('ASO HA ACABAT AAAAAAAAA!!!!');
                        return [2 /*return*/];
                }
            });
        });
    };
    NetEventIO_Client.prototype.parseMessage = function (message) {
        try {
            // initial check
            if (message[message.length - 1] != '$')
                return null;
            // get UUID
            var uuidEnd = message.indexOf('$');
            if (uuidEnd == -1)
                return null;
            var uuid = message.substring(0, uuidEnd);
            message = message.substring(uuidEnd + 1);
            // get metadata
            var packetInitializationEnd = message.indexOf('$');
            if (packetInitializationEnd == -1)
                return null;
            var metadata = message.substring(0, packetInitializationEnd).split(':');
            if (metadata.length != 3)
                return null;
            // get message
            message = message.substring(packetInitializationEnd + 1, (message.length - 1));
            var newMessage = new Message();
            newMessage.id = uuid;
            newMessage.packetType = parseInt(metadata[0]);
            newMessage.eventName = metadata[1];
            newMessage.command = metadata[2];
            newMessage.message = message;
            return newMessage;
        }
        catch (e) {
            console.log(e);
            return null;
        }
    }; //parseMessage
    NetEventIO_Client.prototype.parseBinaryMessage = function (message) {
        return __awaiter(this, void 0, void 0, function () {
            var dv, _a, array, headerLength, dec, header, messageLength, binaryMessage, uuidEnd, uuid, packetInitializationEnd, metadata, newMessage, e_1;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        _b.trys.push([0, 3, , 4]);
                        _a = DataView.bind;
                        return [4 /*yield*/, new Response(message).arrayBuffer()];
                    case 1:
                        dv = new (_a.apply(DataView, [void 0, _b.sent()]))();
                        return [4 /*yield*/, new Response(message).arrayBuffer()];
                    case 2:
                        array = _b.sent();
                        headerLength = dv.getInt32(0, true);
                        dec = new TextDecoder();
                        header = dec.decode(array.slice(4, (headerLength + 4)));
                        messageLength = dv.getInt32(headerLength + 4, true);
                        binaryMessage = array.slice((headerLength + 8), (headerLength + 8 + messageLength));
                        /**/
                        // initial check
                        if (header[header.length - 1] != '$')
                            return [2 /*return*/, null];
                        uuidEnd = header.indexOf('$');
                        if (uuidEnd == -1)
                            return [2 /*return*/, null];
                        uuid = header.substring(0, uuidEnd);
                        header = header.substring(uuidEnd + 1);
                        packetInitializationEnd = header.indexOf('$');
                        if (packetInitializationEnd == -1)
                            return [2 /*return*/, null];
                        metadata = header.substring(0, packetInitializationEnd).split(':');
                        if (metadata.length != 3)
                            return [2 /*return*/, null];
                        newMessage = new BinaryMessage();
                        newMessage.id = uuid;
                        newMessage.packetType = parseInt(metadata[0]);
                        newMessage.eventName = metadata[1];
                        newMessage.command = metadata[2];
                        newMessage.message = new Blob([binaryMessage]);
                        return [2 /*return*/, newMessage];
                    case 3:
                        e_1 = _b.sent();
                        console.log(e_1);
                        return [2 /*return*/, null];
                    case 4: return [2 /*return*/];
                }
            });
        });
    }; //parseBinaryMessage
    NetEventIO_Client.prototype.serializeMessage = function (m) {
        try {
            var str = "";
            str += m.id
                + '$'
                + m.packetType + ':'
                + m.eventName + ':'
                + m.command
                + '$' + m.message + '$';
            return str;
        }
        catch (e) {
            console.log(e);
            return null;
        }
    }; //serializeMessage
    NetEventIO_Client.prototype.getNewMessage = function () {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.client.onmessage = function (data) {
                resolve(data.data);
            };
            _this.client.onerror = function () {
                reject();
            };
        });
    };
    ;
    NetEventIO_Client.prototype.receiveMessage = function () {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.client.onmessage = function (data) {
                resolve(_this.parseMessage(data.data));
            };
            _this.client.onerror = function () {
                reject();
            };
        });
    };
    ;
    NetEventIO_Client.prototype.sendMessage = function (m) {
        var serializedMessage = this.serializeMessage(m);
        this.client.send(serializedMessage);
    };
    NetEventIO_Client.prototype.handleInit = function () {
        return __awaiter(this, void 0, void 0, function () {
            var m, response;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        m = new Message(null, PacketType.C_init, "", "", this.CURRENT_VERSION.toString());
                        this.sendMessage(m);
                        return [4 /*yield*/, this.receiveMessage()];
                    case 1:
                        response = _a.sent();
                        if (response == null)
                            return [2 /*return*/, false];
                        if (response.packetType == PacketType.S_initOk) {
                            this.id = response.message;
                            this.state = NetEventIOClientState.Initialized;
                            return [2 /*return*/, true];
                        }
                        return [2 /*return*/, false];
                }
            });
        });
    };
    NetEventIO_Client.prototype.handleAuth = function () {
        return __awaiter(this, void 0, void 0, function () {
            var token, response, pwd, response;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.onTokenLogin()];
                    case 1:
                        token = _a.sent();
                        if (!(token !== null && token !== '')) return [3 /*break*/, 3];
                        this.sendMessage(new Message(null, PacketType.C_authSend, "", "token", token));
                        return [4 /*yield*/, this.receiveMessage()];
                    case 2:
                        response = _a.sent();
                        if (response == null)
                            return [2 /*return*/, false];
                        if (response.packetType == PacketType.S_authOk) {
                            this.state = NetEventIOClientState.Authenticated;
                            return [2 /*return*/, true];
                        }
                        _a.label = 3;
                    case 3: return [4 /*yield*/, this.onPasswordLogin()];
                    case 4:
                        pwd = _a.sent();
                        this.sendMessage(new Message(null, PacketType.C_authSend, "", "", pwd));
                        return [4 /*yield*/, this.receiveMessage()];
                    case 5:
                        response = _a.sent();
                        if (response == null)
                            return [2 /*return*/, false];
                        if (response.packetType == PacketType.S_authOk) {
                            this.state = NetEventIOClientState.Authenticated;
                            return [2 /*return*/, true];
                        }
                        return [2 /*return*/, false];
                }
            });
        });
    };
    NetEventIO_Client.prototype.handleEvent = function (m) {
        if (this.state != NetEventIOClientState.Authenticated)
            return;
        // search event by name in the dictionary
        try {
            (this.messageListeners[m.eventName])(this, m);
        }
        catch (e) {
            console.log(e);
            return;
        }
    };
    NetEventIO_Client.prototype.handleBinaryEvent = function (bm) {
        if (this.state != NetEventIOClientState.Authenticated)
            return;
        // search event by name in the dictionary
        try {
            (this.messageListeners[bm.eventName])(this, bm);
        }
        catch (e) {
            console.log(e);
            return;
        }
    };
    NetEventIO_Client.prototype.on = function (eventName, listener) {
        if (eventName.indexOf(':') != -1 || eventName.indexOf('$') != -1 || listener == null)
            return false;
        try {
            this.messageListeners[eventName] = listener;
        }
        catch (e) {
            console.log(e);
            return false;
        }
        return true;
    };
    NetEventIO_Client.prototype.removeEvent = function (eventName) {
        if (eventName.indexOf(':') != -1 || eventName.indexOf('$') != -1)
            return false;
        try {
            delete this.messageListeners[eventName];
            return true;
        }
        catch (e) {
            console.log(e);
            return false;
        }
    };
    NetEventIO_Client.prototype.handleCallbackResponse = function (m) {
        var f = this.callbackQueue[m.id];
        if (f == null || f == undefined)
            return;
        delete this.callbackQueue[m.id];
        f(this, m.message);
    };
    NetEventIO_Client.prototype.emit = function (eventName, command, message, callback) {
        if (eventName === void 0) { eventName = ''; }
        if (command === void 0) { command = ''; }
        if (message === void 0) { message = ''; }
        if (this.state != NetEventIOClientState.Authenticated)
            return;
        if (callback == undefined || callback == null) {
            var m = new Message(null, PacketType.C_sendEvent, eventName, command, message);
            var mSerialized = this.serializeMessage(m);
            this.client.send(mSerialized);
        }
        else {
            var m = new Message(null, PacketType.C_sendEventForCallback, eventName, command, message);
            var mSerialized = this.serializeMessage(m);
            this.callbackQueue[m.id] = callback;
            this.client.send(mSerialized);
        }
    };
    NetEventIO_Client.prototype.emitBinary = function (eventName, command, message) {
        if (eventName === void 0) { eventName = ''; }
        if (command === void 0) { command = ''; }
        if (this.state != NetEventIOClientState.Authenticated)
            return;
        var bm = new BinaryMessage(null, PacketType.C_binaryEvent, eventName, command, message);
        var mSerialized = this.serializeBinaryMessage(bm);
        this.client.send(mSerialized);
    };
    NetEventIO_Client.prototype.serializeBinaryMessage = function (bm) {
        // get the text header
        var header = '';
        header += bm.id + '$' + bm.packetType + ':' + bm.eventName + ':' + bm.command + '$';
        // get the footer
        var footer = '$';
        // get the header and message lengths
        var headerLength = header.length;
        var bufHeaderLength = new ArrayBuffer(4);
        var viewHeaderLength = new DataView(bufHeaderLength);
        viewHeaderLength.setUint32(0, headerLength, false);
        var messageLength = bm.message.size;
        var bufMessageLength = new ArrayBuffer(4);
        var viewMessageLength = new DataView(bufMessageLength);
        viewMessageLength.setUint32(0, messageLength, false);
        // append on a new blob
        var b = new Blob([this.toBytesInt32(headerLength), header, this.toBytesInt32(messageLength), bm.message, footer]);
        // return the result
        return b;
    };
    NetEventIO_Client.prototype.toBytesInt32 = function (num) {
        var arr = new Uint8Array([
            (num & 0x000000ff),
            (num & 0x0000ff00) >> 8,
            (num & 0x00ff0000) >> 16,
            (num & 0xff000000) >> 24
        ]);
        return arr.buffer;
    };
    NetEventIO_Client.prototype.subscribe = function (topicName, listener) {
        if (topicName.indexOf(':') >= 0 || topicName.indexOf('$') >= 0 || listener == null)
            return;
        try {
            this.subscriptionListeners[topicName] = listener;
            var m = new Message(null, PacketType.C_subscribe, topicName, "", "");
            this.sendMessage(m);
        }
        catch (Exception) {
            return;
        }
    };
    NetEventIO_Client.prototype.unsubscribe = function (topicName) {
        if (topicName.indexOf(':') >= 0 || topicName.indexOf('$') >= 0)
            return;
        try {
            delete this.subscriptionListeners[topicName];
            var m = new Message(null, PacketType.C_unsubscribe, topicName, "", "");
            this.sendMessage(m);
        }
        catch (e) {
            console.log(e);
            return;
        }
    };
    NetEventIO_Client.prototype.unsubscribeAll = function () {
        this.subscriptionListeners = {};
        try {
            var m = new Message(null, PacketType.C_unsubscribeAll, "", "", "");
            this.sendMessage(m);
        }
        catch (e) {
            console.log(e);
            return;
        }
    };
    NetEventIO_Client.prototype.handleSubscriptionPush = function (m) {
        if (this.state != NetEventIOClientState.Authenticated)
            return;
        // search event by name in the dictionary
        var listener;
        try {
            listener = this.subscriptionListeners[m.eventName];
            // emit event
            var changeType = parseInt(m.command.split(',')[0]);
            var pos = parseInt(m.command.split(',')[1]);
            listener(this, m.eventName, changeType, pos, m.message);
        }
        catch (e) {
            console.log(e);
            return;
        }
        if (listener == null)
            return;
    };
    return NetEventIO_Client;
}());
exports.NetEventIO_Client = NetEventIO_Client;
//# sourceMappingURL=socketevents.io.js.map