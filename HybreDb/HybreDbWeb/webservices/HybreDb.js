module.exports = (function() {

    return require("roadie").WebService.extend({
        create: function(ctx) {
            this.inherited().create.call(this, ctx);
            this.hybre = this.ctx._server.hybre;
        },
        execute: function () {
            var self = this;
            var m = this.ctx.request.parameter("cmd");
            
            this.jsonBody(function (err, json) {
                if (err) return this.send(null, err);

                self.hybre.send(m, json, function(d) {
                    self.sendHybreResult(d);
                });
            });
        },
        list: function () {
            var tab = this.ctx.request.parameter("table");
            var self = this;

            this.hybre.send("list", { table: tab }, function(d) {
                self.sendHybreResult(d);
            });
        },
        // Table name in the url, data, columns and rows given in the body
        createTable: function() {
            var tab = this.ctx.request.parameter("table");
            var self = this;

            this.jsonBody(function(err, o) {
                if (err) return this.send(null, err);

                self.hybre.send("tableCreate", {
                    table: tab,
                    columns: o.columns,
                    rows: o.rows
                }, function(d) {
                    self.send(d);
                });
            });
  
        },
        jsonBody: function(cb) {
            this.requestAnimationFrame.body(function(dat) {
                var o;
                try {
                    o = JSON.parse(dat.toString());
                } catch (e) {
                    return cb({ code: "INV-JSON", message: "Invalid JSON" });
                }
                return cb(null, o);
            });
        },
        sendHybreResult: function(dat) {
            var statuscode = dat.error ? 500 : 200;
            this.ctx.response.status(statuscode);
            this.ctx.response.data(JSON.stringify(dat));
            this.ctx.response.send();
        },
        send: function(dat, err, statuscode) {
            statuscode = statuscode || (err ? 500 : 200);
            this.ctx.response.status(statuscode);
            this.ctx.response.data(JSON.stringify({ error: err, data: dat }));
            this.ctx.response.send();
        }
    });


})();