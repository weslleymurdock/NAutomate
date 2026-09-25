window.nautomateFlow = {
    enableDragging: function () {
        document.querySelectorAll(".flow-node").forEach(function (node) {
            if (node.dataset.dragReady === "true") return;
            node.dataset.dragReady = "true";
            let startX = 0, startY = 0, originX = 0, originY = 0;
            node.addEventListener("pointerdown", function (event) {
                if (event.target.closest("button")) return;
                node.setPointerCapture(event.pointerId);
                startX = event.clientX; startY = event.clientY;
                originX = parseFloat(node.style.left || "0"); originY = parseFloat(node.style.top || "0");
            });
            node.addEventListener("pointermove", function (event) {
                if (!node.hasPointerCapture(event.pointerId)) return;
                node.style.left = (originX + event.clientX - startX) + "px";
                node.style.top = (originY + event.clientY - startY) + "px";
                window.nautomateFlow.updateLinks();
            });
        });
    },
    updateLinks: function () {
        document.querySelectorAll(".flow-link").forEach(function (line) {
            const from = document.getElementById("node-" + line.dataset.from);
            const to = document.getElementById("node-" + line.dataset.to);
            if (!from || !to) return;
            line.setAttribute("x1", from.offsetLeft + from.offsetWidth / 2);
            line.setAttribute("y1", from.offsetTop + from.offsetHeight);
            line.setAttribute("x2", to.offsetLeft + to.offsetWidth / 2);
            line.setAttribute("y2", to.offsetTop);
            line.setAttribute("stroke", "currentColor");
            line.setAttribute("stroke-width", "2");
            line.setAttribute("marker-end", "url(#arrow)");
        });
    }
};
