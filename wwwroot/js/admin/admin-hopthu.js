/* ============================================================
   HỘP THƯ GÓP Ý & YÊU CẦU TRỢ GIÚP — Lọc (Admin)
   - Tìm kiếm theo tên người gửi
   - Lọc theo khoảng ngày gửi
   - Lọc theo trạng thái xử lý
   ============================================================ */
(function () {
    'use strict';

    var table = document.getElementById('dataTable');
    if (!table) return;

    var tbody = table.tBodies[0];
    var searchEl = document.getElementById('searchBox');
    var dateFromEl = document.getElementById('dateFrom');
    var dateToEl = document.getElementById('dateTo');
    var statusEl = document.getElementById('statusFilter');
    var btnResetEl = document.getElementById('btnReset');
    var rowCountEl = document.getElementById('rowCount');
    var totalCountEl = document.getElementById('totalCount');

    // Gom dữ liệu từ các dòng
    var allRows = [];
    var dataRows = tbody.querySelectorAll('tr[data-ngay]');
    for (var i = 0; i < dataRows.length; i++) {
        var tr = dataRows[i];
        allRows.push({
            tr: tr,
            name: (tr.getAttribute('data-ten') || '').toLowerCase(),
            date: tr.getAttribute('data-ngay') || '',            // yyyy-MM-dd
            status: tr.getAttribute('data-trangthai') || ''
        });
    }

    function val(el) { return el ? el.value : ''; }

    function render() {
        var q = val(searchEl).toLowerCase().trim();
        var from = val(dateFromEl);
        var to = val(dateToEl);
        var status = val(statusEl);

        var visible = 0;
        for (var i = 0; i < allRows.length; i++) {
            var r = allRows[i];
            var show = true;
            if (q && r.name.indexOf(q) === -1) show = false;
            if (show && from && r.date && r.date < from) show = false;
            if (show && to && r.date && r.date > to) show = false;
            if (show && status && r.status !== status) show = false;
            r.tr.style.display = show ? '' : 'none';
            if (show) visible++;
        }

        if (rowCountEl) rowCountEl.textContent = visible;
        if (totalCountEl) totalCountEl.textContent = allRows.length;
    }

    if (searchEl) searchEl.addEventListener('input', render);
    if (dateFromEl) dateFromEl.addEventListener('change', render);
    if (dateToEl) dateToEl.addEventListener('change', render);
    if (statusEl) statusEl.addEventListener('change', render);
    if (btnResetEl) btnResetEl.addEventListener('click', function () {
        if (searchEl) searchEl.value = '';
        if (dateFromEl) dateFromEl.value = '';
        if (dateToEl) dateToEl.value = '';
        if (statusEl) statusEl.selectedIndex = 0;
        render();
    });

    // Chạy lần đầu (khởi tạo đếm)
    if (totalCountEl) totalCountEl.textContent = allRows.length;
    render();
})();