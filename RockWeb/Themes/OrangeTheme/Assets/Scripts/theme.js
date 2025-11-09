function BindNavEvents() {
  $(document).ready(function() {
    const bodyElement = $('body');
    const modalOverlay = $('.sidebar-modal-overlay');

    // Initialize state - sidebar closed by default
    bodyElement.addClass('navbar-side-close').removeClass('navbar-side-open');

    // Simple hamburger toggle - just toggle classes
    $('.navbar-toggle-side-left').off('click').on('click', function(e) {
      e.preventDefault();
      e.stopPropagation();

      if (bodyElement.hasClass('navbar-side-open')) {
        bodyElement.removeClass('navbar-side-open').addClass('navbar-side-close');
        modalOverlay.removeClass('active');
      } else {
        bodyElement.removeClass('navbar-side-close').addClass('navbar-side-open');
        modalOverlay.addClass('active');
      }
    });

    // Close menu when clicking overlay
    modalOverlay.off('click').on('click', function(e) {
      e.preventDefault();
      bodyElement.removeClass('navbar-side-open').addClass('navbar-side-close');
      modalOverlay.removeClass('active');
    });

    // Close menu with Escape key
    $(document).on('keyup', function(e) {
    if (e.key === "Escape" && bodyElement.hasClass('navbar-side-open')) {
        bodyElement.removeClass('navbar-side-open').addClass('navbar-side-close');
        modalOverlay.removeClass('active');
    }
    });

    // Menu item clicks - handles sub-navigation
    $('.navbar-side > li.nav-item-top.has-children > .item-title').off('click').on("click", function(e) {
      e.preventDefault(); // Prevent default link behavior
      e.stopPropagation();
      const parentLi = $(this).parent('.nav-item-top');
      const isOpen = parentLi.hasClass('open');

      // Close all other menu items first
      $('.navbar-side > li.nav-item-top').removeClass('open');

      if (!isOpen) {
        // Open this menu item
        parentLi.addClass('open');
      } else {
        // Close this menu item
        parentLi.removeClass('open');
      }
    });

    // Click outside to close sub-navigation
    $('#content-wrapper').on("click", function() {
      bodyElement.removeClass('nav-open');
      $('.navbar-static-side').removeClass('open-secondary-nav');
      $('.navbar-side li').removeClass('open');
    });

    // Handle modal interactions
    if ($('#fixed-header').length) {
      if ($('#fixed-header').find('.modal.in').length) {
        bodyElement.addClass('navbar-side-open');
        $('#fixed-header').find('.modal.in').on('hidden.bs.modal', function(e) {
          bodyElement.removeClass('navbar-side-open');
        });
      }
    }

    // Initialize top header offset
    topHeaderOffset();

    // Watch for header size changes
    var topHeader = $('.rock-top-header');
    if (topHeader.length) {
      var topHeaderResizeObserver = new ResizeObserver(function(entries) {
        topHeaderOffset();
      });
      topHeaderResizeObserver.observe(topHeader[0]);
    }
  });
}

function topHeaderOffset() {
  var topHeader = document.querySelector('.rock-top-header');
  if (topHeader) {
    document.body.style.setProperty('--top-header-height', topHeader.offsetHeight + 'px');

    var topHeaderStyle = window.getComputedStyle(topHeader);
    if (topHeaderStyle.position !== 'relative') {
      document.body.style.setProperty('--sticky-element-offset', topHeader.offsetHeight + 'px');
    } else {
      document.body.style.setProperty('--sticky-element-offset', '0px');
    }
  }
}

function PreventNumberScroll() {
  $(document).ready(function() {
    // Disable mousewheel on number input fields when in focus
    // (to prevent Chromium browsers from changing the value when scrolling)
    $('form').on('focus', 'input[type=number]', function(e) {
      $(this).on('mousewheel.disableScroll', function(e) {
        e.preventDefault();
      });
    });

    $('form').on('blur', 'input[type=number]', function(e) {
      $(this).off('mousewheel.disableScroll');
    });

    $('form').on('keydown', 'input[type=number]', function(e) {
      if (e.which === 38 || e.which === 40) {
        e.preventDefault();
      }
    });
  });

  // Handle note editor interactions
  $('.js-note-editor .meta-body').on("focusin", function() {
    var noteBody = $(this);
    var height = noteBody.prop('scrollHeight');
    noteBody.addClass("focus-within").css('height', height);

    noteBody.on('webkitTransitionEnd otransitionend oTransitionEnd msTransitionEnd transitionend', function(e) {
      if ($(this).hasClass("focus-within")) {
        noteBody.addClass("overflow-visible");
      }
    });

    ResizeTextarea();

    unfocusOnClickOutside(this, function() {
      noteBody.removeClass("focus-within overflow-visible").css('height', '');
    });
  });
}

function unfocusOnClickOutside(element, callback) {
  $(document).on("click.unfocus", function(e) {
    if (!$(element).is(e.target) && $(element).has(e.target).length === 0) {
      callback();
      // Disable the event handler to avoid unwanted behaviors
      $(document).off(".unfocus");
    }
  });
}

function ResizeTextarea() {
  $('.js-notetext').on("input", function() {
    // Resize textarea
    var textarea = $(this);
    textarea.css('height', 'auto').css('height', textarea.prop('scrollHeight'));

    // Get closest note-editor and resize it
    var noteEditor = textarea.closest('.focus-within').addClass("no-transition");
    noteEditor.css('height', 'auto').css('height', noteEditor.prop('scrollHeight'));
  });
}

// Fixes an issue with the wait spinner caused by browser Back/Forward caching
function HandleBackForwardCache() {
  // Forcibly hide the wait spinner and clear pending request if page is reloaded from bfcache
  // (Currently WebKit only - Safari browsers prior to v13 have a known bug)
  window.addEventListener('pageshow', function(e) {
    if (e.persisted) {
      document.querySelector('#updateProgress').style.display = 'none';

      // Check if the page is in postback, and if so, reset the PageRequestManager state
      if (Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack()) {
        // Reset the PageRequestManager state and manually clear the request object
        Sys.WebForms.PageRequestManager.getInstance()._processingRequest = false;
        Sys.WebForms.PageRequestManager.getInstance()._request = null;
      }
    }
  });
}
