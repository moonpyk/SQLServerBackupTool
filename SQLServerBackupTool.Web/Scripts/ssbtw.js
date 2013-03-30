window.ssbt = {
    messages: {
        CONFIRM_DELETE_BACKUP: "Are you sure you want to delete this database backup ?",
        CONFIRM_PURGE_BACKUPS: "Are you sure you want to purge expired backups ?"
    }
};

$(document).ready(function () {
    var $f = $('#form-aft');

    $('.pldr').pldr({ autostart: false });
    $('.loading').modal({ show: false, keyboard: false });

    var pleaseWait = function (message) {
        var me = $('.loading');

        me.find('.message').html(message);

        me.off().on('show', function () {
            me.find('.pldr').pldr('start');
        }).on('hidden', function () {
            me.find('.pldr').pldr('stop');
        }).modal('show');

        return me;
    };

    $('#backup-purge').on('click', function (e) {
        e.preventDefault();

        if (!confirm(ssbt.messages.CONFIRM_PURGE_BACKUPS)) {
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

        var wait = pleaseWait('Please wait while your backup is done...');

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

        if (!confirm(ssbt.messages.CONFIRM_DELETE_BACKUP)) {
            return;
        }

        var $me = $(this),
            $tbody = $me.parents('tbody'),
            href = $me.attr('href');

        if ($tbody.find('tr').length == 1) {
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
});