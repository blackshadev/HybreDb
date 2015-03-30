
module.exports = require("roadie").WebService.extend({
	create: function() {
		this.inherited().create.apply(this, arguments);

		this.ctx.response.header("Content-Type", "application/json");
		this.ctx.response.header("Cache-Control", "no-chache");

	},
	error: function(err) {
		this.ctx.response.status(500);
		this.send(err);
	},
	send: function(d) {
		if(typeof(d) !== "object") d = [d];

		this.ctx.response.data(d);
		this.ctx.response.send();
	}
});