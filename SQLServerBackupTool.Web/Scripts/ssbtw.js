window.ssbt = {
    messages: {
        CONFIRM_DELETE_BACKUP: "Are you sure you want to delete this database backup ?",
        CONFIRM_PURGE_BACKUPS: "Are you sure you want to purge expired backups ?"
    }
};

$(document).ready(function () {
    var $f = $('#form-aft');

    $('.pldr').pldr({ autostart: false });

    $('#backup-purge').on('click', function (e) {
        e.preventDefault();

        if (!confirm(ssbt.messages.CONFIRM_PURGE_BACKUPS)) {
            return;
        }

        var $me = $(this),
            href = $me.attr('href');

        $f.attr('action', href).submit();
    });

    $('.backup-delete').on('click', function (e) {
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