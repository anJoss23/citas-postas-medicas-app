// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Password show/hide toggles
document.addEventListener("click", (event) => {
  const button = event.target.closest(".js-toggle-password");
  if (!button) return;

  const group = button.closest(".input-group");
  const input = group ? group.querySelector(".js-password") : null;
  if (!input) return;

  const isHidden = input.getAttribute("type") === "password";
  input.setAttribute("type", isHidden ? "text" : "password");

  const label = button.querySelector(".js-eye");
  if (label) {
    label.textContent = isHidden ? "Ocultar" : "Ver";
  }
});
