﻿var fs = require("fs");
var dot = require("dot");

module.exports = (function () {
    var pages = {
        "/view/" : "Home",
        "/view/tables": "Tables",
        "/view/execute": "Execute"
    };

    var template = dot.template(fs.readFileSync("./template/template.dot.jst", undefined, { pages: pages }));

    var templateCache = {};

    return require("roadie").WebService.extend({
        pageName: "",
        pageTemplate: null,
        data: null,
        dataDone: false,
        templateDone: false,
        create: function(ctx) {
            this.inherited().create.call(this, ctx);

            this.hybre = ctx._server.hybre;

            var self = this;

            this.pageName = ctx.request.parameter("page") || "index".toLowerCase();
            var fn = this.dataFns[this.pageName] || function(cb) { cb(); };
            this.data = fn.call(this, function () {
                self.dataDone = true;
                self.send();
            });

            this.fetchPageTemplate(function() {
                self.templateDone = true;
                self.send();
            });
        },
        fetchPageTemplate: function (cb) {
            this.pageTemplate = templateCache[this.pageName];

            if (this.pageTemplate) return cb();
            
            var pageTitle = this.pageName[0].toUpperCase() + this.pageName.substr(1);
            var self = this;
            fs.readFile("./views/" + this.pageName + ".html", function (err, dat) {
                if (err) return self.ctx.error(err);
                
                self.pageTemplate = dot.template(template({ page: dat.toString(), pageTitle: pageTitle }));
                templateCache["index"] = self.pageTemplate;
                cb();
            });
        },
        send: function () {
            if (!(this.dataDone && this.templateDone)) return;
            this.ctx.response.header("Content-Type", "text/html");
            this.ctx.response.data(this.pageTemplate(this.data));
            this.ctx.response.send();
        },
        dataFns: {
            tables: function (cb) {
                var self = this;
                this.hybre.send("listTables", {}, function(o) {
                    self.data = o;
                    cb();
                });
            }
        }


    });
})();