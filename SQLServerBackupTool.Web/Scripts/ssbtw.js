(function (exports) {
    exports.ssbt = exports.ssbt || {};
    exports.ssbt.lang = exports.ssbt.lang || {};
    exports.ssbt.currentLang = "en";

    function getMessage(m) {
        var loc = exports.ssbt.lang[exports.ssbt.currentLang];

        if (typeof loc === "object" && typeof loc[m] === "string" && loc[m] !== "") {
            return loc[m];
        }
        return m;
    };

    exports._ = function (m) {
        return getMessage(m);
    };

}(window));
$(document).ready(function () {
    ssbt.currentLang = $('html').attr('lang');

    // Multiusage antiforgery token protected form
    var $f = $('#form-aft');

    /**
     * Index page, with backups
     */

    $('.pldr').pldr({ autostart: false });
    $('.loading').modal({
        show: false,
        keyboard: false
    });

    $('.select2').each(function () {
        var $me = $(this);

        $me.select2({
            placeholder: $me.data('placeholder'),
            width: '220px'
        });
    });

    var pleaseWait = function (message) {
        var $me = $('.loading');

        $me.find('.message').html(message);

        $me.off().on('show', function () {
            $me.find('.pldr').pldr('start');
        }).on('hidden', function () {
            $me.find('.pldr').pldr('stop');
        }).modal('show');

        return $me;
    };

    $('#backup-purge').on('click', function (e) {
        e.preventDefault();

        if (!confirm(_('Are you sure you want to purge expired backups ?'))) {
            return;
        }

        var $me = $(this),
            href = $me.attr('href');

        $f.attr('action', href).submit();
    });

    $('.backup-do').on('click', function (e) {
        e.preventDefault();
        var $me = $(this),
            $bContainer = $('#backups-container');

        var wait = pleaseWait(_('Please wait while your backup is beeing done...'));

        $.ajax({
            type: 'post',
            url: $me.attr('href'),
            data: $f.serialize()
        }).done(function (html) {
            $bContainer.find('.no-row').remove();
            $bContainer.append(html);

        }).always(function () {
            wait.modal('hide');
        });
    });

    $(document).on('click', '.backup-delete', function (e) {
        e.preventDefault();

        if (!confirm(_('Are you sure you want to delete this database backup ?'))) {
            return;
        }

        var $me = $(this),
            $tbody = $me.parents('tbody'),
            href = $me.attr('href');

        if ($tbody.find('tr').length === 1) {
            $f.attr('action', href).submit();
            return;
        }

        $.ajax({
            url: href,
            type: 'post',
            data: $f.serialize()
        }).done(function (r) {
            if (r == "OK") {
                $me.parents('tr').remove();
            }
        });
    });

    /**
     * Users management
     */

    $('.user-delete').on('click', function (e) {
        e.preventDefault();

        var $me = $(this),
            href = $me.attr('href');

        $f.attr('action', href).submit();
    });

    $('#password-generate').on('click', function (e) {
        e.preventDefault();
        $.ajax({
            url: '/Users/GeneratePassword'
        }).done(function (pw) {
            $('input.pw').attr('type', 'text').val(pw);
        });
    });

    $('#btn-changepw').on('click', function (e) {
        e.preventDefault();
        $(this).toggleClass('active');
        $('.changepw').slideToggle();
    });
});
