var currentContent = "users";
var currentNumberContent = "1";

jQuery(document).ready(function () {
	relayButtons("btn-primary", "btn-outline-secondary");

	filter("users");
	filter("roles");
	filter("reports");
	filter("categories");
	filter("sections");
	filter("themes");
	
	relayDroprownItems();
	search();
});

function relayButtons(activeButtonClassName, inactiveButtonClassName) {
	var relayButtons = (document).getElementsByClassName("relay-button");
	for (let i = 0; i < relayButtons.length; i++) {
		relayButtons[i].addEventListener('click', () => {
			// Смена подсцветки кнопки
			for (let j = 0; j < relayButtons.length; j++) {
				relayButtons[j].classList.remove(activeButtonClassName);
				relayButtons[j].classList.add(inactiveButtonClassName);
			}
			relayButtons[i].classList.remove(inactiveButtonClassName);
			relayButtons[i].classList.add(activeButtonClassName);
		});
	}

	$("#users-button").click(() => {
		currentContent = "users";
		currentNumberContent = "1";
		hideAllAnotherContent("users");
	});
	$("#roles-button").click(() => {
		currentContent = "roles";
		currentNumberContent = "2";
		hideAllAnotherContent("roles");
	});
	$("#reports-button").click(() => {
		currentContent = "reports";
		currentNumberContent = "3";
		hideAllAnotherContent("reports");
	});
	$("#categories-button").click(() => {
		currentContent = "categories";
		currentNumberContent = "4";
		hideAllAnotherContent("categories");
	});
	$("#sections-button").click(() => {
		currentContent = "sections";
		currentNumberContent = "5";
		hideAllAnotherContent("sections");
	});
	$("#themes-button").click(() => {
		currentContent = "themes";
		currentNumberContent = "6";
		hideAllAnotherContent("themes");
	});
}

function hideAllAnotherContent(name) {
	var relayContents = (document).getElementsByClassName("relay-content");
	for (let i = 0; i < relayContents.length; i++) {
		relayContents[i].style.display = "none";
	}
	var activeContent = (document).getElementById(name + "-content");
	if (activeContent != null)
		activeContent.style.display = "block";

	// Смена полей поиска
	reinitializeSearchFieldAfterChangingPage(name);
}

function filter(name) {
	$("#" + name + "-sort-button").click(() => {
		var parameterCheckboxes = $("#" + name + "-checkboxes input[type='checkbox']");
		parameterCheckboxes.each(function (i, checkbox) {
			var query = "#" + name + "-table #" + $(checkbox).prop("id");
			if ($(checkbox).prop("checked")) {
				$(query).each(function (i, cell) {
					$(cell).css("display", "table-cell");
				})
			}
			else {
				$(query).each(function (i, cell) {
					$(cell).css("display", "none");
				})
			}
		});
	});
}

function hideOrShowDescription() {
	var showBtns = $(".description .description-show-btn");
	var hideBtns = $(".description .description-hide-btn");
	var descriptionContents = $(".description .description-content");
	for (let i = 0; i < showBtns.length; i++) {
		showBtns[i].addEventListener("click", () => {
			descriptionContents[i].style.display = "block";
			showBtns[i].style.display = "none";
		});
		hideBtns[i].addEventListener("click", () => {
			descriptionContents[i].style.display = "none";
			showBtns[i].style.display = "block";
		});
	}
}

function relayDroprownItems() {
	var dropdownItems = $("#search-dropdown-menu > .dropdown-item");
	dropdownItems.each((i, item) => {
		item.addEventListener("click", () => {
			dropdownItems.each((i, elem) => {
				$(elem).removeClass("active");
			});
			$(item).addClass("active");
			$("#about-search-type").text("Поиск " + item.innerText.toLowerCase());
		});
	});
}

function reinitializeSearchFieldAfterChangingPage(name) {
	function disableJQueryElems() {
		for (let i = 0; i < arguments.length; i++) {
			$(arguments[i]).prop("disabled", true);
		}
	}
	function enableJQueryElems() {
		for (let i = 0; i < arguments.length; i++) {
			$(arguments[i]).prop("disabled", false);
		}
	}
	function setDropdownItemsByIdAndInnerText(pairs) {
		var oldDropdownItems = $("#search-field #search-dropdown-menu > .dropdown-item");
		oldDropdownItems.each((i, item) => {
			item.remove();
		});
		for (let i = 0; i < pairs.length; i++) {
			searchDropdownMenu.append("<button id='" + pairs[i][0] + "' class='dropdown-item'>" + pairs[i][1] + "</button>");
		}
		relayDroprownItems();
	}

	var searchField = $("#search-field");
	var searchTextInput = $("#search-field #search-text-input");
	var searchBtn = $("#search-field #search-btn");
	var searchDropdownMenuBtn = $("#search-field #dropdown-btn");
	var searchDropdownMenu = $("#search-field #search-dropdown-menu");
	var aboutSearchType = $("#about-search-type");
	var notSearchAccessMessage = "Здесь поиск недоступен";
	var searchAccessMessage = "Выберите критерий поиска";

	if (name == "users") {
		enableJQueryElems(searchTextInput, searchBtn, searchDropdownMenuBtn);
		aboutSearchType.text(searchAccessMessage);
		setDropdownItemsByIdAndInnerText([
			["users-search-for-user_name", "По имени"],
			["users-search-for-email", "По Email"],
			["users-search-for-status", "По статусу"],
			["users-search-for-roles", "По роли"]
		]);
	}
	if (name == "roles") {
		disableJQueryElems(searchTextInput, searchBtn, searchDropdownMenuBtn);
		aboutSearchType.text(notSearchAccessMessage);
	}
	if (name == "reports") {
		enableJQueryElems(searchTextInput, searchBtn, searchDropdownMenuBtn);
		aboutSearchType.text(searchAccessMessage);
		setDropdownItemsByIdAndInnerText([
			["reports-search-for-object", "По объекту"],
			["reports-search-for-type", "По типу"],
			["reports-search-for-initiator", "По иницатору"],
			["reports-search-for-description", "По описанию"]
		]);
	}
	if (name == "categories") {
		enableJQueryElems(searchTextInput, searchBtn, searchDropdownMenuBtn);
		aboutSearchType.text(searchAccessMessage);
		setDropdownItemsByIdAndInnerText([
			["categories-search-for-title", "По названию"],
			["categories-search-for-sections", "По разделу"]
		]);
	}
	if (name == "sections") {
		enableJQueryElems(searchTextInput, searchBtn, searchDropdownMenuBtn);
		aboutSearchType.text(searchAccessMessage);
		setDropdownItemsByIdAndInnerText([
			["sections-search-for-title", "По названию"],
			["sections-search-for-description", "По описанию"],
			["sections-search-for-themes", "По теме"]
		]);
	}
	if (name == "themes") {
		enableJQueryElems(searchTextInput, searchBtn, searchDropdownMenuBtn);
		aboutSearchType.text(searchAccessMessage);
		setDropdownItemsByIdAndInnerText([
			["themes-search-for-title", "По названию"],
			["themes-search-for-description", "По описанию"],
			["themes-search-for-creator", "По создателю"]
		]);
	}
}

function search() {
	$("#search-btn").click(() => {
		var searchActiveItem = $("#search-field #search-dropdown-menu > .dropdown-item.active");
		var searchText = $("#search-field #search-text-input").val();
		if (searchActiveItem.length == 1) {
			$("#search-text-input").removeClass("is-invalid");
			var searchField = searchActiveItem.prop("id").split("-")[3];			
			var query = "#" + currentContent + "-content tr";
			var trs = $(query);
			var cells = $("#" + currentContent + "-table tr td#" + searchField + "-" + currentNumberContent);
			// Для themes поля description немного другая логика из-за кнопок показать/скрыть
			if (currentContent == "themes" && searchField == "description") {
				cells = $("#" + currentContent + "-table tr td#" + searchField + "-" + currentNumberContent + " .description-content");
			}
			for (let i = 0; i < cells.length; i++) {
				if (cells[i].innerText.toLowerCase().indexOf(searchText.toLowerCase()) == -1) {
					trs[i + 1].style.display = "none";
				}
				else {
					trs[i + 1].style.display = "table-row";
				}
			}
		}
		else {
			$("#search-text-input").addClass("is-invalid");
			$("#about-search-type").removeClass("text-muted");
			$("#about-search-type").addClass("text-danger");
		}
	});
	$("#search-dropdown-menu").click(() => {
		$("#search-text-input").removeClass("is-invalid");
		$("#about-search-type").addClass("text-muted");
		$("#about-search-type").removeClass("text-danger");
	});
}

function startLoadingAnim() {
	var loadingElem = $("#loading");
	loadingElem.show();
}

function stopLoadingAnim() {
	var loadingElem = $("#loading");
	loadingElem.hide();
}