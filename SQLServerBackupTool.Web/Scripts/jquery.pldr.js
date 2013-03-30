/*!
 * jQuery pldr plugin
 * Original author: @moonpyk
 * Licensed under the MIT license
 */
(function ($) {
    var defaults = {
        speed: 9,
        width: 50,
        height: 50,
        totalFrames: 8,
        frameWidth: 50,
        imageSrc: '/Content/img/loader.png',
        autostart: true
    };

    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return $(this).each(function () {
                var state = {
                    index: 0,
                    xPos: 0,
                    sbf: 0, // Seconds between frames
                    timer: null
                },
                me = this;

                $(me).data('pldr_settings', settings).data('pldr_state', state);

                me.style.backgroundImage = 'url(' + settings.imageSrc + ')';
                me.style.width = settings.width + 'px';
                me.style.height = settings.width + 'px';

                if (settings.autostart) {
                    methods.start.apply(this);
                }
            });
        },
        start: function () {
            return $(this).each(function () {
                var me = this,
                    $me = $(me),
                    settings = $me.data('pldr_settings'),
                    state = $me.data('pldr_state');

                if (settings == null || state == null) {
                    return;
                }

                state.sbf = 1 / (Math.round(100 / settings.speed));

                var animCallback = function () {
                    state.xPos += settings.frameWidth;
                    state.index++;

                    if (state.index >= settings.totalFrames) {
                        state.xPos = 0;
                        state.index = 0;
                    }

                    me.style.backgroundPosition = (-state.xPos) + 'px 0';
                    state.timer = setTimeout(animCallback, state.sbf * 1000);
                };

                state.timer = setTimeout(animCallback, state.sbf / 1000);
            });
        },
        stop: function () {
            return $(this).each(function () {
                var $me = $(this),
                    state = $me.data('pldr_state');

                if (state == null || state.timer == null) {
                    return;
                }

                clearTimeout(state.timer);
                state.timer = null;
            });
        }
    };
    $.fn.pldr = function (method) {
        // Method calling logic
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            $.error('Method "' + method + '" does not exist on jQuery.pldr');
            return null;
        }
    };
    $.fn.pldr.defaults = defaults;
})(jQuery);
