window.ssbt = {
    messages: {
        CONFIRM_DELETE_BACKUP: 'Are you sure you want to delete this database backup ?',
        CONFIRM_PURGE_BACKUPS: 'Are you sure you want to purge expired backups ?',
        WAIT_WHILE_BACKUP: 'Please wait while your backup is beeing done...'
    }
};

$(document).ready(function () {
    /**
     * Index page, with backups
     */

    var $f = $('#form-aft');

    $('.pldr').pldr({ autostart: false });
    $('.loading').modal({ show: false, keyboard: false });

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

        var wait = pleaseWait(ssbt.messages.WAIT_WHILE_BACKUP);

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