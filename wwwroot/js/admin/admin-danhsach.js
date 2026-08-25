/* ============================================================
   DANH SÁCH ỦNG HỘ — Lọc, sắp xếp và phân trang (Admin)
   Dùng cho: Quỹ Vì người nghèo / Cứu trợ / Biển Đảo
   Số dòng mỗi trang: 20
   ============================================================ */
(function () {
    'use strict';

    var PAGE_SIZE = 20;

    var table = document.getElementById('dataTable');
    if (!table) return;

    var tbody = table.tBodies[0];
    var searchEl = document.getElementById('searchBox');
    var dateFromEl = document.getElementById('dateFrom');
    var dateToEl = document.getElementById('dateTo');
    var sortEl = document.getElementById('sortSelect');
    var btnResetEl = document.getElementById('btnReset');
    var rowCountEl = document.getElementById('rowCount');
    var totalCountEl = document.getElementById('totalCount');
    var paginationEl = document.getElementById('pagination');
    var pagesWrapEl = document.getElementById('pagesWrap');
    var pagePrevEl = document.getElementById('pagePrev');
    var pageNextEl = document.getElementById('pageNext');

    // Gom dữ liệu từ các dòng (mỗi dòng có data-ngay, data-money, data-ten)
    var allRows = [];
    var dataRows = tbody.querySelectorAll('tr[data-ngay]');
    for (var i = 0; i < dataRows.length; i++) {
        var tr = dataRows[i];
        allRows.push({
            tr: tr,
            name: (tr.getAttribute('data-ten') || '').toLowerCase(),
            date: tr.getAttribute('data-ngay') || '',              // yyyy-MM-dd
            money: parseFloat(tr.getAttribute('data-money')) || 0
        });
    }

    var currentList = [];
    var currentPage = 1;

    function val(el) { return el ? el.value : ''; }

    // Lọc + sắp xếp
    function compute() {
        var q = val(searchEl).toLowerCase().trim();
        var from = val(dateFromEl);
        var to = val(dateToEl);
        var sort = val(sortEl) || 'moiNhat';

        var list = allRows.filter(function (r) {
            if (q && r.name.indexOf(q) === -1) return false;
            if (from && r.date && r.date < from) return false;
            if (to && r.date && r.date > to) return false;
            return true;
        });

        list.sort(function (a, b) {
            switch (sort) {
                case 'tienGiam':  return (b.money - a.money) || a.name.localeCompare(b.name);
                case 'tienTang':  return (a.money - b.money) || a.name.localeCompare(b.name);
                case 'tenAZ':     return a.name.localeCompare(b.name);
                case 'tenZA':     return b.name.localeCompare(a.name);
                case 'cuNhat':    return (a.date || '').localeCompare(b.date || '');
                case 'moiNhat':
                default:          return (b.date || '').localeCompare(a.date || '');
            }
        });
        return list;
    }

    function makeEllipsis() {
        var s = document.createElement('span');
        s.className = 'page-ellipsis';
        s.textContent = '…';
        return s;
    }

    function makePageBtn(p, label) {
        var b = document.createElement('button');
        b.type = 'button';
        b.className = 'page-btn' + (p === currentPage ? ' is-active' : '');
        b.textContent = label;
        b.onclick = function () {
            currentPage = p;
            render();
            var top = (table.offsetTop || 0) - 30;
            window.scrollTo({ top: top, behavior: 'smooth' });
        };
        return b;
    }

    function renderPageButtons() {
        var total = currentList.length;
        var totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
        if (!pagesWrapEl) return totalPages;

        pagesWrapEl.innerHTML = '';
        var startP, endP, windowSize = 1;

        if (totalPages <= 7) {
            startP = 1; endP = totalPages;
        } else {
            startP = Math.max(1, currentPage - windowSize);
            endP = Math.min(totalPages, currentPage + windowSize);
            if (currentPage <= windowSize + 2) { startP = 1; endP = 5; }
            else if (currentPage >= totalPages - windowSize - 1) { startP = totalPages - 4; endP = totalPages; }
        }

        if (startP > 1) {
            pagesWrapEl.appendChild(makePageBtn(1, '1'));
            if (startP > 2) pagesWrapEl.appendChild(makeEllipsis());
        }
        for (var p = startP; p <= endP; p++) {
            pagesWrapEl.appendChild(makePageBtn(p, String(p)));
        }
        if (endP < totalPages) {
            if (endP < totalPages - 1) pagesWrapEl.appendChild(makeEllipsis());
            pagesWrapEl.appendChild(makePageBtn(totalPages, String(totalPages)));
        }
        return totalPages;
    }

    function render() {
        var total = currentList.length;
        var totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
        if (currentPage > totalPages) currentPage = totalPages;
        var start = (currentPage - 1) * PAGE_SIZE;
        var end = Math.min(start + PAGE_SIZE, total);

        // Ẩn tất cả dòng dữ liệu
        for (var i = 0; i < allRows.length; i++) allRows[i].tr.style.display = 'none';

        // Đưa các dòng của trang hiện tại lên cuối bảng (đúng thứ tự sắp xếp)
        var frag = document.createDocumentFragment();
        for (var j = start; j < end; j++) {
            var tr = currentList[j].tr;
            tr.style.display = '';
            tr.children[0].textContent = j + 1; // số thứ tự liên tục
            frag.appendChild(tr);
        }
        tbody.appendChild(frag);

        // Trạng thái không có kết quả
        var emptyRow = tbody.querySelector('#rowEmpty');
        if (total === 0) {
            if (!emptyRow) {
                emptyRow = document.createElement('tr');
                emptyRow.id = 'rowEmpty';
                emptyRow.innerHTML =
                    '<td colspan="5"><div class="admin-empty">' +
                    '<i class="fas fa-filter"></i>' +
                    '<p>Không tìm thấy kết quả phù hợp.</p></div></td>';
                tbody.appendChild(emptyRow);
            }
            tbody.appendChild(emptyRow);
            emptyRow.style.display = '';
        } else if (emptyRow) {
            emptyRow.style.display = 'none';
        }

        // Cập nhật thông tin hiển thị
        if (rowCountEl) rowCountEl.textContent = end - start;
        if (totalCountEl) totalCountEl.textContent = total;

        // Phân trang
        if (paginationEl) paginationEl.style.display = totalPages > 1 ? '' : 'none';
        if (pagePrevEl) pagePrevEl.disabled = currentPage <= 1;
        if (pageNextEl) pageNextEl.disabled = currentPage >= totalPages;
        renderPageButtons();
    }

    function refresh() {
        currentList = compute();
        currentPage = 1;
        render();
    }

    if (searchEl) searchEl.addEventListener('input', refresh);
    if (dateFromEl) dateFromEl.addEventListener('change', refresh);
    if (dateToEl) dateToEl.addEventListener('change', refresh);
    if (sortEl) sortEl.addEventListener('change', refresh);
    if (pagePrevEl) pagePrevEl.addEventListener('click', function () { if (currentPage > 1) { currentPage--; render(); } });
    if (pageNextEl) pageNextEl.addEventListener('click', function () { currentPage++; render(); });
    if (btnResetEl) btnResetEl.addEventListener('click', function () {
        if (searchEl) searchEl.value = '';
        if (dateFromEl) dateFromEl.value = '';
        if (dateToEl) dateToEl.value = '';
        if (sortEl) sortEl.value = 'moiNhat';
        refresh();
    });

    // Chạy lần đầu để áp dụng phân trang (20 dòng/trang)
    refresh();
})();