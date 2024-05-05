'use strict';

var vlaExtension = require('@somfic/vla-extension');

class TestExtension extends vlaExtension.Extension {
    async on_start(handle) {
        handle.log("test_extension");
    }
    async on_stop(handle) { }
}
class TestExtension2 {
    async on_start(handle) {
        handle.log("test_extension2");
    }
    async on_stop(handle) { }
}

exports.TestExtension = TestExtension;
exports.TestExtension2 = TestExtension2;
