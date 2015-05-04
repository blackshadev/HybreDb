module.exports = (function() {

    return require("roadie").WebService.extend({
        create: function(ctx) {
            this.inherited().create.call(this, ctx);
            this.hybre = this.ctx._server.hybre;
        },
        execute: function () {
            var self = this;
            var m = this.ctx.request.parameter("cmd");
            this.ctx.request.body(function(b) {
                var json;

                try {
                    json = JSON.parse(b.toString());
                } catch (e) {
                    self.send(null, { code: "INV-JSON", message: "Invalid JSON" });
                    return;
                }

                this.hybre.send(m, json, function(d) {
                    self.send(d);
                });
            });
        },
        list: function() {
            var tab = this.ctx.request.parameter("table");
            var self = this;

            this.hybre.send("list", { table: tab }, function(d) {
                self.send(d);
            });

        },
        send: function(dat, err, statuscode) {
            statuscode = statuscode || (err ? 500 : 200);
            this.ctx.response.status(statuscode);
            this.ctx.response.data(JSON.stringify({ error: err, data: dat }));
            this.ctx.response.send();
        }
    });


})();