var WebService = require("roadie").WebService;
var fs = require("fs");

module.exports = (function() {

    var StaticService = WebService.extend({
        root: "../statics/", // path to statics
        mimeType: "text/plain",
        isOpen: false,
        error: null,
        create: function(ctx) {
            this.inherited().create.call(this, ctx);

            this.fname = this.ctx.request.uri;
            this.root = ctx.cwd() + "/" + this.root;
            this.mimeType = StaticService.GuessMimeType(this.fname);

            this.open();
        },
        open: function() {
            var f = this.root + "/" + this.fname;
            console.log("[Statics] opening " + f);

            fs.readFile(f, this.read.bind(this));

        },
        read: function(err, dat) {
            this.isOpen = true;

            this.error = err;
            this._data = dat;
            this.send();
        },
        send: function() {
            if(this.error) return this.ctx.error(this.error);
            this.ctx.response.header("Content-Type", this.mimeType);
            this.ctx.response.header("Content-Type", this.mimeType);
            this.ctx.response.data(this._data);
            this.ctx.response.send();
        },
        html: function() {
            console.log("Html served");
        }
    });
    StaticService.GuessMimeType = function(fname) {
        var ext = fname.slice(fname.lastIndexOf(".") + 1);
        switch(ext) {
            case "json": return "application/json";
            case "xml": return "application/xml";
            case "pdf": return "application/pdf";
            case "htm": case "html": return "text/html";
            case "css": return "text/css";
            case "js": return "application/javascript";
            case "gif": return "image/gif";
            case "jpg": case "jpeg": return "image/jpeg";
            case "png": return "image/png";
            default: return "text/plain";
        }
    };

    return StaticService;
})();