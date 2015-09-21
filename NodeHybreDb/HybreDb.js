var $o = require("./core.js");
var net = require("net");
var EventEmitter = require("events").EventEmitter;

module.exports = (function() {

    // 
    var HybreDbConnector = $o.Object.extend({
        port: 4242,
        host: "127.0.0.1",
        socket: null,
        connected: false,
        state: null,
        _callbacks: null,
        _data: null, // object holding the data buffer
        _offset: 0,
        create: function(port, host) {
            this.port = port || this.port;
            this.host = host || this.host;
            this.events = new EventEmitter();
            this.state = HybreDbConnector.States.None;
            this._callbacks = [];
        },
        start: function() {
            var self = this;
            this.socket = new net.connect({
                port: this.port,
                host: this.host
            }, function() {
                self.connected = true;
                self.events.emit("ready", self);
            });

            this.socket.on("data", function(dat) { self.processData(dat); });
        },
        send: function (cmd, params, cb) {
            if (!this.connected) throw "Server not yet connected";

            var str = JSON.stringify({ method: cmd, params: params });
            var data = new Buffer(str, "utf16le");

            var b = new Buffer(4);
            b.writeInt32LE(data.length, 0);

            this.socket.write(b);
            this.socket.write(data);

            this._callbacks.push(cb);
        },
        processData: function (data, offs) {
            offs = offs || 0;

            if (this.state === HybreDbConnector.States.None) {
                var i = data.readInt32LE(0);
                this._data = new Buffer(i);
                this.state = HybreDbConnector.States.WaitingForData;
                this.processData(data, 4);
            } else {
                data.copy(this._data, this._offset, offs, data.length);
                this._offset += data.length - offs;

                if (this._data.length === this._offset) {
                    var str = this._data.toString("utf16le");
                    this.events.emit("data", str);

                    this._data = null;
                    this._offset = 0;
                    this.state = HybreDbConnector.States.None;
                    var fn = this._callbacks.shift();
                    fn(JSON.parse(str));
                }
            }

        },
        close: function() {
            this.socket.end();
            this.socket.destroy();
        }
    });
    HybreDbConnector.States = {
        None: 0,
        WaitingForData: 1 // after getting the length of a message
    };

    return HybreDbConnector;
})();