/**
 * NewsEditor - Trình soạn thảo nội dung bài viết tự viết
 * Hoàn toàn miễn phí, không cần API key, không phụ thuộc CDN bên ngoài
 * Dựa trên contenteditable HTML5
 */
(function (window) {
    'use strict';

    /**
     * Khởi tạo editor
     * @param {string} textareaId - ID của textarea gốc
     * @param {object} options - Cấu hình
     */
    function NewsEditor(textareaId, options) {
        this.textarea = document.getElementById(textareaId);
        if (!this.textarea) return;

        this.options = Object.assign({
            uploadUrl: null,
            height: 500,
            placeholder: 'Nhập nội dung bài viết...'
        }, options || {});

        this._build();
        this._bindEvents();
        this._syncFromTextarea();
    }

    /**
     * Xây dựng giao diện editor
     */
    NewsEditor.prototype._build = function () {
        var self = this;

        // Ẩn textarea gốc
        this.textarea.style.display = 'none';

        // Tạo wrapper
        this.wrapper = document.createElement('div');
        this.wrapper.className = 'news-editor';
        this.wrapper.style.cssText = 'border:1px solid #ddd;border-radius:8px;overflow:hidden;background:#fff;';

        // Tạo toolbar
        this.toolbar = document.createElement('div');
        this.toolbar.className = 'news-editor-toolbar';
        this.toolbar.style.cssText = 'display:flex;flex-wrap:wrap;gap:2px;padding:8px;border-bottom:1px solid #ddd;background:#f8f9fa;align-items:center;';

        // Tạo content area
        this.content = document.createElement('div');
        this.content.className = 'news-editor-content';
        this.content.contentEditable = 'true';
        this.content.style.cssText = 'min-height:' + this.options.height + 'px;padding:16px;font-family:"Be Vietnam Pro",Arial,sans-serif;font-size:15px;line-height:1.7;color:#333;outline:none;overflow-y:auto;';
        this.content.setAttribute('data-placeholder', this.options.placeholder);

        // Tạo status bar
        this.statusbar = document.createElement('div');
        this.statusbar.className = 'news-editor-statusbar';
        this.statusbar.style.cssText = 'padding:4px 12px;border-top:1px solid #ddd;background:#f8f9fa;font-size:12px;color:#888;display:flex;justify-content:space-between;align-items:center;';

        var wordCount = document.createElement('span');
        wordCount.className = 'news-editor-wordcount';
        wordCount.textContent = '0 từ';
        this.statusbar.appendChild(wordCount);

        var statusText = document.createElement('span');
        statusText.textContent = 'Sẵn sàng';
        this.statusbar.appendChild(statusText);

        // Thêm vào DOM
        this.wrapper.appendChild(this.toolbar);
        this.wrapper.appendChild(this.content);
        this.wrapper.appendChild(this.statusbar);
        this.textarea.parentNode.insertBefore(this.wrapper, this.textarea.nextSibling);

        // Xây dựng toolbar
        this._buildToolbar();

        // Thêm CSS cho placeholder
        this._addPlaceholderStyle();
    };

    /**
     * Xây dựng toolbar với các nút chức năng
     */
    NewsEditor.prototype._buildToolbar = function () {
        var self = this;

        var groups = [
            // Nhóm 1: Undo/Redo
            [
                { icon: '↩', title: 'Hoàn tác', action: function () { document.execCommand('undo'); } },
                { icon: '↪', title: 'Làm lại', action: function () { document.execCommand('redo'); } }
            ],
            // Nhóm 2: Định dạng văn bản
            [
                { icon: '<b>B</b>', title: 'In đậm', action: function () { document.execCommand('bold'); }, active: 'bold' },
                { icon: '<i>I</i>', title: 'In nghiêng', action: function () { document.execCommand('italic'); }, active: 'italic' },
                { icon: '<u>U</u>', title: 'Gạch chân', action: function () { document.execCommand('underline'); }, active: 'underline' },
                { icon: '<s>S</s>', title: 'Gạch ngang', action: function () { document.execCommand('strikeThrough'); }, active: 'strikeThrough' }
            ],
            // Nhóm 3: Màu chữ
            [
                { type: 'color', icon: 'A', title: 'Màu chữ', action: function (color) { document.execCommand('foreColor', false, color); } },
                { type: 'bgcolor', icon: '▣', title: 'Màu nền', action: function (color) { document.execCommand('hiliteColor', false, color); } }
            ],
            // Nhóm 4: Căn lề
            [
                { icon: '⯇', title: 'Căn trái', action: function () { document.execCommand('justifyLeft'); }, active: 'justifyLeft' },
                { icon: '☰', title: 'Căn giữa', action: function () { document.execCommand('justifyCenter'); }, active: 'justifyCenter' },
                { icon: '⯈', title: 'Căn phải', action: function () { document.execCommand('justifyRight'); }, active: 'justifyRight' },
                { icon: '☷', title: 'Căn đều', action: function () { document.execCommand('justifyFull'); }, active: 'justifyFull' }
            ],
            // Nhóm 5: Danh sách
            [
                { icon: '•', title: 'Danh sách gạch đầu dòng', action: function () { document.execCommand('insertUnorderedList'); }, active: 'insertUnorderedList' },
                { icon: '1.', title: 'Danh sách đánh số', action: function () { document.execCommand('insertOrderedList'); }, active: 'insertOrderedList' },
                { icon: '↘', title: 'Giảm thụt lề', action: function () { document.execCommand('outdent'); } },
                { icon: '↗', title: 'Tăng thụt lề', action: function () { document.execCommand('indent'); } }
            ],
            // Nhóm 6: Chèn
            [
                { icon: '🔗', title: 'Chèn liên kết', action: function () { self._insertLink(); } },
                { icon: '🖼', title: 'Chèn ảnh', action: function () { self._insertImage(); } },
                { icon: '🎬', title: 'Chèn video', action: function () { self._insertVideo(); } },
                { icon: '▦', title: 'Chèn bảng', action: function () { self._insertTable(); } }
            ],
            // Nhóm 7: Khác
            [
                { icon: '⌫', title: 'Xóa định dạng', action: function () { document.execCommand('removeFormat'); } },
                { icon: '</>', title: 'Xem mã HTML', action: function () { self._toggleCodeView(); } },
                { icon: '⛶', title: 'Toàn màn hình', action: function () { self._toggleFullscreen(); } }
            ]
        ];

        groups.forEach(function (group, groupIndex) {
            if (groupIndex > 0) {
                var separator = document.createElement('div');
                separator.style.cssText = 'width:1px;height:24px;background:#ddd;margin:0 4px;';
                this.toolbar.appendChild(separator);
            }

            group.forEach(function (btn) {
                var button = document.createElement('button');
                button.type = 'button';
                button.innerHTML = btn.icon;
                button.title = btn.title;
                button.style.cssText = 'width:32px;height:32px;border:1px solid transparent;border-radius:4px;background:transparent;cursor:pointer;font-size:14px;display:flex;align-items:center;justify-content:center;color:#555;transition:all 0.2s;';
                button.addEventListener('mouseenter', function () {
                    button.style.background = '#e9ecef';
                });
                button.addEventListener('mouseleave', function () {
                    if (!button.classList.contains('active')) {
                        button.style.background = 'transparent';
                    }
                });

                if (btn.type === 'color' || btn.type === 'bgcolor') {
                    // Nút màu - hiển thị dropdown màu
                    var colorInput = document.createElement('input');
                    colorInput.type = 'color';
                    colorInput.style.cssText = 'position:absolute;opacity:0;width:0;height:0;';
                    colorInput.addEventListener('change', function () {
                        btn.action(colorInput.value);
                        colorInput.value = '#000000';
                    });
                    button.appendChild(colorInput);
                    button.style.position = 'relative';
                    button.addEventListener('click', function (e) {
                        e.preventDefault();
                        colorInput.click();
                    });
                    // Thêm gạch màu dưới chữ A
                    if (btn.type === 'color') {
                        button.style.borderBottom = '3px solid #000';
                    } else {
                        button.style.background = '#ffff00';
                    }
                } else {
                    button.addEventListener('click', function () {
                        btn.action();
                        self.content.focus();
                    });
                }

                if (btn.active) {
                    button.dataset.command = btn.active;
                }

                this.toolbar.appendChild(button);
            }, this);
        }, this);

        // Thêm dropdown chọn font size
        var sizeSeparator = document.createElement('div');
        sizeSeparator.style.cssText = 'width:1px;height:24px;background:#ddd;margin:0 4px;';
        this.toolbar.appendChild(sizeSeparator);

        var sizeSelect = document.createElement('select');
        sizeSelect.style.cssText = 'height:32px;border:1px solid #ddd;border-radius:4px;padding:0 4px;font-size:13px;background:#fff;color:#555;cursor:pointer;';
        var sizes = [
            { value: '3', label: 'Cỡ chữ' },
            { value: '1', label: 'Nhỏ' },
            { value: '2', label: 'Vừa' },
            { value: '3', label: 'Bình thường' },
            { value: '4', label: 'Lớn' },
            { value: '5', label: 'Rất lớn' },
            { value: '6', label: 'Cực lớn' },
            { value: '7', label: 'Khổng lồ' }
        ];
        sizes.forEach(function (s) {
            var opt = document.createElement('option');
            opt.value = s.value;
            opt.textContent = s.label;
            sizeSelect.appendChild(opt);
        });
        sizeSelect.addEventListener('change', function () {
            if (sizeSelect.value) {
                document.execCommand('fontSize', false, sizeSelect.value);
                sizeSelect.value = '3';
            }
        });
        this.toolbar.appendChild(sizeSelect);

        // Thêm dropdown chọn font family
        var fontSelect = document.createElement('select');
        fontSelect.style.cssText = 'height:32px;border:1px solid #ddd;border-radius:4px;padding:0 4px;font-size:13px;background:#fff;color:#555;cursor:pointer;max-width:140px;';
        var fonts = [
            { value: '', label: 'Font chữ' },
            { value: 'Arial, sans-serif', label: 'Arial' },
            { value: 'Georgia, serif', label: 'Georgia' },
            { value: '"Times New Roman", serif', label: 'Times New Roman' },
            { value: 'Courier New, monospace', label: 'Courier New' },
            { value: '"Be Vietnam Pro", Arial, sans-serif', label: 'Be Vietnam Pro' },
            { value: 'Tahoma, sans-serif', label: 'Tahoma' },
            { value: 'Verdana, sans-serif', label: 'Verdana' }
        ];
        fonts.forEach(function (f) {
            var opt = document.createElement('option');
            opt.value = f.value;
            opt.textContent = f.label;
            fontSelect.appendChild(opt);
        });
        fontSelect.addEventListener('change', function () {
            if (fontSelect.value) {
                document.execCommand('fontName', false, fontSelect.value);
                fontSelect.value = '';
            }
        });
        this.toolbar.appendChild(fontSelect);
    };

    /**
     * Thêm CSS chung cho editor + placeholder + figure + resize handles
     * LƯU Ý: .news-figure KHÔNG dùng overflow:hidden để resize handles và toolbar hiển thị được
     */
    NewsEditor.prototype._addPlaceholderStyle = function () {
        var style = document.createElement('style');
        style.textContent = 
            '.news-editor-content:empty:before{content:attr(data-placeholder);color:#aaa;pointer-events:none;}' +
            '.news-figure{display:inline-block;margin:16px 0;max-width:100%;text-align:center;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.08);transition:box-shadow 0.3s;position:relative;}' +
            '.news-figure:hover{box-shadow:0 4px 16px rgba(0,0,0,0.12);}' +
            '.news-figure img{display:block;max-width:100%;height:auto;border-radius:8px 8px 0 0;}' +
            '.news-figure iframe{display:block;max-width:100%;border-radius:8px 8px 0 0;}' +
            '.news-figure video{display:block;max-width:100%;border-radius:8px 8px 0 0;}' +
            '.news-figure figcaption{display:block;padding:8px 12px;background:#f8f9fa;font-size:13px;color:#555;font-style:italic;border-top:1px solid #eee;text-align:center;min-height:20px;cursor:text;}' +
            '.news-figure figcaption:empty:before{content:"Nhập mô tả...";color:#aaa;pointer-events:none;}' +
            '.news-figure figcaption:focus{outline:2px dashed #17a2b8;outline-offset:-2px;background:#fff;}' +
            '.news-figure-selected{outline:3px solid #17a2b8;outline-offset:3px;border-radius:10px;}' +
            '.news-media-selected{opacity:0.9;}' +
            '.news-resize-handle{position:absolute;width:12px;height:12px;background:#17a2b8;border:2px solid #fff;border-radius:50%;z-index:1000;}' +
            '.news-resize-nw{top:-8px;left:-8px;cursor:nwse-resize;}' +
            '.news-resize-ne{top:-8px;right:-8px;cursor:nesw-resize;}' +
            '.news-resize-sw{bottom:-8px;left:-8px;cursor:nesw-resize;}' +
            '.news-resize-se{bottom:-8px;right:-8px;cursor:nwse-resize;}' +
            '.news-media-toolbar{position:absolute;top:-40px;left:0;background:#333;color:#fff;border-radius:4px;padding:4px 8px;display:flex;gap:8px;z-index:1001;font-size:12px;align-items:center;box-shadow:0 2px 8px rgba(0,0,0,0.2);}' +
            '.news-media-toolbar button{background:transparent;border:none;color:#fff;cursor:pointer;font-size:12px;padding:2px 6px;border-radius:3px;transition:background 0.2s;}' +
            '.news-media-toolbar button:hover{background:#555;}' +
            '.news-media-toolbar .delete-btn{color:#ff6b6b;}' +
            '.news-context-menu{position:fixed;background:#fff;border:1px solid #ddd;border-radius:8px;box-shadow:0 4px 16px rgba(0,0,0,0.15);z-index:10000;min-width:200px;padding:6px;}' +
            '.news-context-menu-item{display:block;width:100%;padding:8px 12px;border:none;background:transparent;text-align:left;font-size:13px;color:#333;cursor:pointer;border-radius:4px;transition:background 0.2s;font-family:inherit;}' +
            '.news-context-menu-item:hover{background:#f0f0f0;}' +
            '.news-context-menu-item.danger{color:#ff6b6b;}' +
            '.news-context-menu-item.danger:hover{background:#ffe0e0;}' +
            '.news-caption-delete{position:absolute;top:4px;right:4px;width:20px;height:20px;border:none;border-radius:50%;background:#ff6b6b;color:#fff;font-size:11px;cursor:pointer;display:none;align-items:center;justify-content:center;z-index:10;line-height:1;padding:0;}' +
            '.news-caption-delete:hover{background:#e05555;}' +
            '.news-caption-wrap{position:relative;}' +
            '.news-figure figcaption{text-align:center !important;}';
        document.head.appendChild(style);
    };

    /**
     * Gắn sự kiện
     */
    NewsEditor.prototype._bindEvents = function () {
        var self = this;

        // Cập nhật trạng thái nút active khi chọn văn bản
        this.content.addEventListener('keyup', function () { self._updateToolbarState(); });
        this.content.addEventListener('mouseup', function () { self._updateToolbarState(); });
        this.content.addEventListener('input', function () { self._syncToTextarea(); self._updateWordCount(); });

        // Click phải vào ảnh/video để hiển thị menu ngữ cảnh (Kích thước, Mô tả, Xóa)
        this.content.addEventListener('contextmenu', function (e) {
            var target = e.target;
            if (target.tagName === 'IMG' || target.tagName === 'IFRAME' || target.tagName === 'VIDEO') {
                e.preventDefault();
                e.stopPropagation();
                self._showContextMenu(e, target);
            }
        });

        // Click vào ảnh/video - không chọn (tránh tự động play video)
        this.content.addEventListener('click', function (e) {
            var target = e.target;
            if (target.tagName !== 'IMG' && target.tagName !== 'IFRAME' && target.tagName !== 'VIDEO') {
                self._deselectMedia();
            }
        });

        // Click ra ngoài để bỏ chọn và ẩn menu ngữ cảnh
        document.addEventListener('click', function (e) {
            if (!self.content.contains(e.target)) {
                self._deselectMedia();
                self._hideContextMenu();
            }
        });

        // Đồng bộ khi submit form
        var form = this.textarea.form;
        if (form) {
            form.addEventListener('submit', function () {
                self._deselectMedia();
                self._syncToTextarea();
            });
        }
    };

    /**
     * Chọn media (ảnh/video) - chỉ tạo figure wrapper, không hiển thị resize handles
     */
    NewsEditor.prototype._selectMedia = function (media) {
        var self = this;
        this._deselectMedia();

        this._selectedMedia = media;
        media.classList.add('news-media-selected');

        // Tạo wrapper figure nếu chưa có
        var figure = media.closest('figure');
        if (!figure) {
            figure = document.createElement('figure');
            figure.className = 'news-figure';
            media.parentNode.insertBefore(figure, media);
            figure.appendChild(media);
        }
        figure.classList.add('news-figure-selected');
    };

    /**
     * Bỏ chọn media hiện tại
     */
    NewsEditor.prototype._deselectMedia = function () {
        if (this._selectedMedia) {
            this._selectedMedia.classList.remove('news-media-selected');
            this._selectedMedia = null;
        }
        var figures = this.content.querySelectorAll('.news-figure-selected');
        figures.forEach(function (f) {
            f.classList.remove('news-figure-selected');
        });
        if (this._resizeHandles) {
            this._resizeHandles.forEach(function (h) { h.remove(); });
            this._resizeHandles = null;
        }
        if (this._mediaToolbar) {
            this._mediaToolbar.remove();
            this._mediaToolbar = null;
        }
        this._hideContextMenu();
    };

    /**
     * Tạo các handle resize cho media
     */
    NewsEditor.prototype._createResizeHandles = function (media) {
        var self = this;
        this._resizeHandles = [];

        var positions = ['nw', 'ne', 'sw', 'se'];
        positions.forEach(function (pos) {
            var handle = document.createElement('div');
            handle.className = 'news-resize-handle news-resize-' + pos;
            handle.style.cssText = 'position:absolute;width:12px;height:12px;background:#17a2b8;border:2px solid #fff;border-radius:50%;z-index:1000;cursor:' + (pos === 'nw' || pos === 'se' ? 'nwse-resize' : 'nesw-resize') + ';';

            var figure = media.closest('figure');
            if (figure) {
                figure.style.position = 'relative';
                figure.appendChild(handle);
            } else {
                media.parentNode.appendChild(handle);
            }

            handle.addEventListener('mousedown', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self._startResize(e, media, pos);
            });

            self._resizeHandles.push(handle);
        });
    };

    /**
     * Bắt đầu kéo resize
     */
    NewsEditor.prototype._startResize = function (e, media, position) {
        var self = this;
        var figure = media.closest('figure');
        var startX = e.clientX;
        var startY = e.clientY;
        var startWidth = media.offsetWidth;
        var startHeight = media.offsetHeight;
        var aspectRatio = startWidth / startHeight;

        function onMouseMove(e) {
            var dx = e.clientX - startX;
            var dy = e.clientY - startY;
            var newWidth = startWidth;
            var newHeight = startHeight;

            if (position.indexOf('e') !== -1) {
                newWidth = startWidth + dx;
            }
            if (position.indexOf('s') !== -1) {
                newHeight = startHeight + dy;
            }
            if (position.indexOf('w') !== -1) {
                newWidth = startWidth - dx;
            }
            if (position.indexOf('n') !== -1) {
                newHeight = startHeight - dy;
            }

            // Giữ tỷ lệ khung hình
            if (e.shiftKey) {
                if (Math.abs(dx) > Math.abs(dy)) {
                    newHeight = newWidth / aspectRatio;
                } else {
                    newWidth = newHeight * aspectRatio;
                }
            }

            // Giới hạn kích thước tối thiểu
            newWidth = Math.max(50, newWidth);
            newHeight = Math.max(50, newHeight);

            // Giới hạn tối đa theo chiều rộng editor
            var maxWidth = self.content.clientWidth - 40;
            newWidth = Math.min(newWidth, maxWidth);

            media.style.width = newWidth + 'px';
            media.style.height = 'auto';
            if (figure) {
                figure.style.width = newWidth + 'px';
            }
            self._syncToTextarea();
        }

        function onMouseUp() {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
            self._syncToTextarea();
        }

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
    };

    /**
     * Hiển thị menu ngữ cảnh khi nhấn chuột phải vào media
     */
    NewsEditor.prototype._showContextMenu = function (e, media) {
        var self = this;
        this._hideContextMenu();
        this._deselectMedia();

        this._selectedMedia = media;
        media.classList.add('news-media-selected');

        // Tạo wrapper figure nếu chưa có
        var figure = media.closest('figure');
        if (!figure) {
            figure = document.createElement('figure');
            figure.className = 'news-figure';
            media.parentNode.insertBefore(figure, media);
            figure.appendChild(media);
        }
        figure.classList.add('news-figure-selected');

        // Tạo menu ngữ cảnh
        this._contextMenu = document.createElement('div');
        this._contextMenu.className = 'news-context-menu';

        // Nút Kích thước
        var sizeBtn = this._createContextMenuItem('📐 Kích thước', function () {
            // Đóng menu trước rồi bật resize - tránh xung đột với _deselectMedia
            self._hideContextMenu();
            self._enableResize(media);
        });
        this._contextMenu.appendChild(sizeBtn);

        // Nút Mô tả
        var descBtn = this._createContextMenuItem('📝 Mô tả', function () {
            self._hideContextMenu();
            self._toggleDescription(media);
        });
        this._contextMenu.appendChild(descBtn);

        // Nút Xóa
        var deleteBtn = this._createContextMenuItem('🗑️ Xóa', function () {
            if (confirm('Xóa media này?')) {
                figure.remove();
                self._deselectMedia();
                self._syncToTextarea();
            }
            self._hideContextMenu();
        }, true);
        this._contextMenu.appendChild(deleteBtn);

        document.body.appendChild(this._contextMenu);

        // Định vị menu tại vị trí chuột
        var menuWidth = 200;
        var menuHeight = this._contextMenu.offsetHeight;
        var x = e.clientX;
        var y = e.clientY;
        if (x + menuWidth > window.innerWidth) {
            x = window.innerWidth - menuWidth - 10;
        }
        if (y + menuHeight > window.innerHeight) {
            y = window.innerHeight - menuHeight - 10;
        }
        this._contextMenu.style.left = x + 'px';
        this._contextMenu.style.top = y + 'px';
    };

    /**
     * Ẩn menu ngữ cảnh
     */
    NewsEditor.prototype._hideContextMenu = function () {
        if (this._contextMenu) {
            this._contextMenu.remove();
            this._contextMenu = null;
        }
    };

    /**
     * Tạo một item trong menu ngữ cảnh
     */
    NewsEditor.prototype._createContextMenuItem = function (label, action, isDanger) {
        var item = document.createElement('button');
        item.type = 'button';
        item.className = 'news-context-menu-item' + (isDanger ? ' danger' : '');
        item.textContent = label;

        item.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            action();
        });

        return item;
    };

    /**
     * Bật chế độ chỉnh kích thước cho media
     */
    NewsEditor.prototype._enableResize = function (media) {
        var self = this;

        this._selectedMedia = media;
        media.classList.add('news-media-selected');

        // Tạo wrapper figure nếu chưa có
        var figure = media.closest('figure');
        if (!figure) {
            figure = document.createElement('figure');
            figure.className = 'news-figure';
            media.parentNode.insertBefore(figure, media);
            figure.appendChild(media);
        }
        figure.classList.add('news-figure-selected');
        figure.style.position = 'relative';

        // Tạo resize handles
        this._createResizeHandles(media);

        // Tạo toolbar hướng dẫn resize
        this._createResizeToolbar(media, figure);
    };

    /**
     * Tạo toolbar hướng dẫn khi đang resize
     */
    NewsEditor.prototype._createResizeToolbar = function (media, figure) {
        var self = this;
        this._mediaToolbar = document.createElement('div');
        this._mediaToolbar.className = 'news-media-toolbar';
        this._mediaToolbar.style.cssText = 'position:absolute;top:-40px;left:0;background:#333;color:#fff;border-radius:4px;padding:4px 8px;display:flex;gap:8px;z-index:1001;font-size:12px;align-items:center;box-shadow:0 2px 8px rgba(0,0,0,0.2);';

        var infoText = document.createElement('span');
        infoText.textContent = 'Kéo các góc để thay đổi kích thước';
        infoText.style.cssText = 'color:#fff;font-size:11px;white-space:nowrap;';
        this._mediaToolbar.appendChild(infoText);

        var doneBtn = document.createElement('button');
        doneBtn.type = 'button';
        doneBtn.textContent = '✓ Xong';
        doneBtn.style.cssText = 'background:#17a2b8;border:none;color:#fff;cursor:pointer;font-size:12px;padding:2px 8px;border-radius:3px;';
        doneBtn.addEventListener('mouseenter', function () { doneBtn.style.background = '#138496'; });
        doneBtn.addEventListener('mouseleave', function () { doneBtn.style.background = '#17a2b8'; });
        doneBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            self._deselectMedia();
            self._syncToTextarea();
        });
        this._mediaToolbar.appendChild(doneBtn);

        figure.style.position = 'relative';
        figure.appendChild(this._mediaToolbar);
    };

    /**
     * Thêm hoặc sửa mô tả cho media
     */
    NewsEditor.prototype._toggleDescription = function (media) {
        var self = this;
        var figure = media.closest('figure');
        if (!figure) {
            figure = document.createElement('figure');
            figure.className = 'news-figure';
            media.parentNode.insertBefore(figure, media);
            figure.appendChild(media);
        }

        var figcaption = figure.querySelector('figcaption');
        if (figcaption) {
            // Nếu đã có mô tả, focus để sửa
            figcaption.contentEditable = 'true';
            figcaption.focus();
            var range = document.createRange();
            range.selectNodeContents(figcaption);
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
        } else {
            // Tạo wrapper cho mô tả (để đặt nút xóa ở góc trên bên phải của mô tả)
            var captionWrap = document.createElement('div');
            captionWrap.className = 'news-caption-wrap';

            // Tạo mô tả mới
            figcaption = document.createElement('figcaption');
            figcaption.className = 'news-caption';
            figcaption.contentEditable = 'true';
            figcaption.textContent = '';
            captionWrap.appendChild(figcaption);
            figure.appendChild(captionWrap);
            figcaption.focus();
        }

        // Thêm nút xóa mô tả
        this._addDescriptionDeleteButton(figcaption, figure);
        this._syncToTextarea();
    };

    /**
     * Thêm nút xóa cho mô tả
     */
    NewsEditor.prototype._addDescriptionDeleteButton = function (figcaption, figure) {
        var self = this;

        // Tìm nút xóa hiện có hoặc tạo mới
        var deleteBtn = figure.querySelector('.news-caption-delete');
        if (!deleteBtn) {
            deleteBtn = document.createElement('button');
            deleteBtn.type = 'button';
            deleteBtn.className = 'news-caption-delete';
            deleteBtn.textContent = '✕';
            deleteBtn.title = 'Xóa mô tả';

            deleteBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self._removeDescription(figcaption, figure);
            });

            // Đặt nút xóa trong wrapper của mô tả (góc trên bên phải)
            var captionWrap = figcaption.parentNode;
            if (captionWrap && captionWrap.classList.contains('news-caption-wrap')) {
                captionWrap.style.position = 'relative';
                captionWrap.appendChild(deleteBtn);
            } else {
                figure.style.position = 'relative';
                figure.appendChild(deleteBtn);
            }
        }

        // Chỉ thêm event listeners một lần cho figcaption
        if (!figcaption.dataset.hasDeleteListener) {
            figcaption.dataset.hasDeleteListener = 'true';

            // Hiển thị nút xóa khi hover hoặc focus vào mô tả
            figcaption.addEventListener('mouseenter', function () {
                deleteBtn.style.display = 'flex';
            });
            figcaption.addEventListener('mouseleave', function () {
                if (document.activeElement !== figcaption) {
                    deleteBtn.style.display = 'none';
                }
            });
            figcaption.addEventListener('focus', function () {
                deleteBtn.style.display = 'flex';
            });
            figcaption.addEventListener('blur', function () {
                deleteBtn.style.display = 'none';
                // Nếu mô tả trống, tự động xóa
                if (!figcaption.textContent.trim()) {
                    self._removeDescription(figcaption, figure);
                }
                self._syncToTextarea();
            });
        }

        // Hiển thị nút xóa ngay khi vừa tạo
        deleteBtn.style.display = 'flex';
    };

    /**
     * Xóa mô tả của media
     */
    NewsEditor.prototype._removeDescription = function (figcaption, figure) {
        var captionWrap = figcaption ? figcaption.parentNode : null;
        if (figcaption) {
            figcaption.remove();
        }
        // Xóa cả wrapper nếu có
        if (captionWrap && captionWrap.classList.contains('news-caption-wrap')) {
            captionWrap.remove();
        }
        var deleteBtn = figure.querySelector('.news-caption-delete');
        if (deleteBtn) {
            deleteBtn.remove();
        }
        this._syncToTextarea();
    };

    /**
     * Cập nhật trạng thái active của các nút toolbar
     */
    NewsEditor.prototype._updateToolbarState = function () {
        var self = this;
        var buttons = this.toolbar.querySelectorAll('button[data-command]');
        buttons.forEach(function (btn) {
            var command = btn.dataset.command;
            var isActive = false;
            try {
                isActive = document.queryCommandState(command);
            } catch (e) { }
            if (isActive) {
                btn.classList.add('active');
                btn.style.background = '#cce5ff';
                btn.style.borderColor = '#66afe9';
            } else {
                btn.classList.remove('active');
                btn.style.background = 'transparent';
                btn.style.borderColor = 'transparent';
            }
        });
    };

    /**
     * Lấy HTML "sạch" để lưu vào textarea.
     * Loại bỏ các thuộc tính/element chỉ dành cho editor:
     * - contenteditable (trên figcaption) -> để người xem KHÔNG thể chỉnh sửa mô tả
     * - Các nút xóa, resize handles, toolbar, class selection
     */
    NewsEditor.prototype._getCleanHtml = function () {
        // Clone DOM để không ảnh hưởng đến editor đang mở
        var clone = this.content.cloneNode(true);

        // 1. Bỏ contenteditable khỏi mọi phần tử
        var editableEls = clone.querySelectorAll('[contenteditable]');
        editableEls.forEach(function (el) {
            el.removeAttribute('contenteditable');
        });

        // 2. Xóa các element editor-only
        var editorEls = clone.querySelectorAll('.news-caption-delete, .news-resize-handle, .news-media-toolbar, .news-context-menu');
        editorEls.forEach(function (el) {
            el.remove();
        });

        // 3. Bỏ class selection
        var selectedEls = clone.querySelectorAll('.news-figure-selected, .news-media-selected');
        selectedEls.forEach(function (el) {
            el.classList.remove('news-figure-selected', 'news-media-selected');
        });

        // 4. Bỏ data attributes editor-only
        var captionEls = clone.querySelectorAll('figcaption[data-has-delete-listener]');
        captionEls.forEach(function (el) {
            el.removeAttribute('data-has-delete-listener');
        });

        // 5. Đảm bảo figcaption không còn contenteditable (lần cuối)
        var captions = clone.querySelectorAll('figcaption');
        captions.forEach(function (el) {
            el.removeAttribute('contenteditable');
        });

        return clone.innerHTML;
    };

    /**
     * Đồng bộ nội dung từ content sang textarea (HTML sạch)
     */
    NewsEditor.prototype._syncToTextarea = function () {
        this.textarea.value = this._getCleanHtml();
    };

    /**
     * Đồng bộ nội dung từ textarea sang content
     */
    NewsEditor.prototype._syncFromTextarea = function () {
        this.content.innerHTML = this.textarea.value || '';
        this._updateWordCount();
    };

    /**
     * Cập nhật số từ
     */
    NewsEditor.prototype._updateWordCount = function () {
        var text = this.content.innerText || '';
        var words = text.trim().split(/\s+/).filter(function (w) { return w.length > 0; });
        var countEl = this.wrapper.querySelector('.news-editor-wordcount');
        if (countEl) {
            countEl.textContent = words.length + ' từ';
        }
    };

    /**
     * Chèn liên kết
     */
    NewsEditor.prototype._insertLink = function () {
        var url = prompt('Nhập URL liên kết:', 'https://');
        if (url && url.trim()) {
            var selectedText = window.getSelection().toString();
            if (selectedText) {
                document.execCommand('createLink', false, url.trim());
            } else {
                document.execCommand('insertHTML', false, '<a href="' + url.trim() + '" target="_blank">' + url.trim() + '</a>');
            }
        }
    };

    /**
     * Chèn ảnh - upload qua API
     */
    NewsEditor.prototype._insertImage = function () {
        var self = this;
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';
        input.onchange = function () {
            var file = this.files[0];
            if (!file) return;

            // Nếu có uploadUrl thì upload qua API
            if (self.options.uploadUrl) {
                var formData = new FormData();
                formData.append('file', file);
                var token = document.querySelector('input[name="__RequestVerificationToken"]');
                if (token) {
                    formData.append('__RequestVerificationToken', token.value);
                }

                // Hiển thị loading
                var loadingText = document.createElement('p');
                loadingText.textContent = 'Đang tải ảnh lên...';
                loadingText.style.cssText = 'color:#888;font-style:italic;';
                self.content.appendChild(loadingText);

                fetch(self.options.uploadUrl, {
                    method: 'POST',
                    body: formData
                })
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    loadingText.remove();
                    if (data.success) {
                        var figureHtml = '<figure class="news-figure" style="display:inline-block;max-width:100%;">' +
                            '<img src="' + data.url + '" alt="' + file.name + '" style="max-width:100%;height:auto;" />' +
                            '</figure><p><br></p>';
                        document.execCommand('insertHTML', false, figureHtml);
                    } else {
                        alert(data.message || 'Upload thất bại');
                    }
                })
                .catch(function (err) {
                    loadingText.remove();
                    alert('Lỗi kết nối: ' + err.message);
                });
            } else {
                // Fallback: dùng FileReader để hiển thị ảnh dạng base64
                var reader = new FileReader();
                reader.onload = function (e) {
                    var figureHtml = '<figure class="news-figure" style="display:inline-block;max-width:100%;">' +
                        '<img src="' + e.target.result + '" alt="' + file.name + '" style="max-width:100%;height:auto;" />' +
                        '</figure><p><br></p>';
                    document.execCommand('insertHTML', false, figureHtml);
                };
                reader.readAsDataURL(file);
            }
        };
        input.click();
    };

    /**
     * Chèn video (embed) - bọc trong figure với caption
     */
    NewsEditor.prototype._insertVideo = function () {
        var self = this;
        var url = prompt('Nhập URL video (YouTube, Vimeo, Facebook...):', 'https://');
        if (!url || !url.trim()) return;

        var embedHtml = this._getVideoEmbed(url.trim());
        if (embedHtml) {
            var figureHtml = '<figure class="news-figure" style="display:inline-block;max-width:100%;">' +
                embedHtml +
                '</figure><p><br></p>';
            document.execCommand('insertHTML', false, figureHtml);
        } else {
            alert('Không thể nhúng video từ đường dẫn này. Vui lòng kiểm tra lại.');
        }
    };

    /**
     * Tạo HTML embed video từ URL
     */
    NewsEditor.prototype._getVideoEmbed = function (url) {
        // YouTube
        var match = url.match(/(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})/);
        if (match) {
            return '<iframe width="100%" height="auto" src="https://www.youtube.com/embed/' + match[1] + '" frameborder="0" allowfullscreen style="width:100%;height:auto;min-height:315px;"></iframe>';
        }
        // Vimeo
        match = url.match(/vimeo\.com\/(\d+)/);
        if (match) {
            return '<iframe src="https://player.vimeo.com/video/' + match[1] + '" width="100%" height="auto" frameborder="0" allowfullscreen style="width:100%;height:auto;min-height:315px;"></iframe>';
        }
        // Facebook
        if (url.includes('facebook.com') || url.includes('fb.watch')) {
            return '<iframe src="https://www.facebook.com/plugins/video.php?href=' + encodeURIComponent(url) + '&show_text=false" width="100%" height="auto" style="border:none;overflow:hidden;width:100%;height:auto;min-height:315px;" scrolling="no" frameborder="0" allowfullscreen="true" allow="autoplay; clipboard-write; encrypted-media; picture-in-picture; web-share"></iframe>';
        }
        // File trực tiếp
        if (url.match(/\.(mp4|webm|ogg)(\?.*)?$/i)) {
            return '<video controls width="100%" style="width:100%;height:auto;"><source src="' + url + '" type="video/mp4">Trình duyệt không hỗ trợ video.</video>';
        }
        return null;
    };

    /**
     * Chèn bảng
     */
    NewsEditor.prototype._insertTable = function () {
        var rows = prompt('Số hàng:', '3');
        var cols = prompt('Số cột:', '3');
        rows = parseInt(rows) || 3;
        cols = parseInt(cols) || 3;

        var html = '<table style="border-collapse:collapse;width:100%;margin:10px 0;">';
        for (var r = 0; r < rows; r++) {
            html += '<tr>';
            for (var c = 0; c < cols; c++) {
                html += '<td style="border:1px solid #ddd;padding:8px;">&nbsp;</td>';
            }
            html += '</tr>';
        }
        html += '</table><p><br></p>';

        document.execCommand('insertHTML', false, html);
    };

    /**
     * Chuyển đổi chế độ xem mã HTML
     */
    NewsEditor.prototype._toggleCodeView = function () {
        if (this.content.contentEditable === 'true') {
            // Chuyển sang chế độ xem code
            this._codeView = document.createElement('textarea');
            this._codeView.value = this._getCleanHtml();
            this._codeView.style.cssText = 'width:100%;min-height:' + this.options.height + 'px;padding:16px;font-family:Consolas,monospace;font-size:13px;border:none;outline:none;resize:vertical;';
            this.content.style.display = 'none';
            this.wrapper.insertBefore(this._codeView, this.content.nextSibling);
            this.content.contentEditable = 'false';
        } else {
            // Chuyển về chế độ soạn thảo
            this.content.innerHTML = this._codeView.value;
            // Khi chuyển về, cần re-enable contenteditable cho figcaption để vẫn chỉnh được mô tả
            var captions = this.content.querySelectorAll('figcaption');
            captions.forEach(function (el) {
                el.setAttribute('contenteditable', 'true');
            });
            this._codeView.remove();
            this._codeView = null;
            this.content.style.display = '';
            this.content.contentEditable = 'true';
            this._syncToTextarea();
        }
    };

    /**
     * Chuyển đổi chế độ toàn màn hình
     */
    NewsEditor.prototype._toggleFullscreen = function () {
        if (!this._isFullscreen) {
            this._isFullscreen = true;
            this._originalParent = this.wrapper.parentNode;
            this._originalNextSibling = this.wrapper.nextSibling;
            document.body.appendChild(this.wrapper);
            this.wrapper.style.cssText += 'position:fixed;top:0;left:0;right:0;bottom:0;z-index:9999;border:none;border-radius:0;display:flex;flex-direction:column;';
            this.content.style.flex = '1';
            this.content.style.minHeight = '0';
        } else {
            this._isFullscreen = false;
            this._originalParent.insertBefore(this.wrapper, this._originalNextSibling);
            this.wrapper.style.cssText = 'border:1px solid #ddd;border-radius:8px;overflow:hidden;background:#fff;';
            this.content.style.flex = '';
            this.content.style.minHeight = this.options.height + 'px';
        }
    };

    /**
     * Lấy nội dung HTML
     */
    NewsEditor.prototype.getValue = function () {
        this._syncToTextarea();
        return this.textarea.value;
    };

    /**
     * Đặt nội dung HTML
     */
    NewsEditor.prototype.setValue = function (html) {
        this.textarea.value = html;
        this.content.innerHTML = html;
        // Khi nạp lại nội dung, re-enable contenteditable cho figcaption để admin có thể chỉnh mô tả
        var captions = this.content.querySelectorAll('figcaption');
        captions.forEach(function (el) {
            el.setAttribute('contenteditable', 'true');
        });
        this._updateWordCount();
    };

    // Export
    window.NewsEditor = NewsEditor;

})(window);