(function () {
    function drawChart(canvasId, series) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !series.some(item => item.points && item.points.length)) {
            return;
        }

        const ratio = window.devicePixelRatio || 1;
        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width * ratio;
        canvas.height = rect.height * ratio;

        const ctx = canvas.getContext("2d");
        ctx.scale(ratio, ratio);
        ctx.clearRect(0, 0, rect.width, rect.height);

        const padding = { left: 48, right: 18, top: 18, bottom: 38 };
        const points = series.flatMap(item => item.points || []);
        const minX = Math.min(...points.map(p => p.x));
        const maxX = Math.max(...points.map(p => p.x));
        const minY = Math.min(...points.map(p => p.y));
        const maxY = Math.max(...points.map(p => p.y));
        const xSpan = maxX - minX || 1;
        const ySpan = maxY - minY || 1;

        function sx(x) {
            return padding.left + ((x - minX) / xSpan) * (rect.width - padding.left - padding.right);
        }

        function sy(y) {
            return rect.height - padding.bottom - ((y - minY) / ySpan) * (rect.height - padding.top - padding.bottom);
        }

        ctx.strokeStyle = "#d9e5c1";
        ctx.lineWidth = 1;
        ctx.font = "12px Segoe UI";
        ctx.fillStyle = "#5b695f";

        for (let i = 0; i <= 4; i++) {
            const x = padding.left + i * (rect.width - padding.left - padding.right) / 4;
            const y = padding.top + i * (rect.height - padding.top - padding.bottom) / 4;
            ctx.beginPath();
            ctx.moveTo(padding.left, y);
            ctx.lineTo(rect.width - padding.right, y);
            ctx.stroke();
            ctx.fillText((maxY - i * ySpan / 4).toFixed(2), 8, y + 4);
            ctx.fillText((minX + i * xSpan / 4).toFixed(2), x - 12, rect.height - 12);
        }

        series.forEach(item => {
            if (!item.points || !item.points.length) {
                return;
            }

            ctx.strokeStyle = item.color;
            ctx.fillStyle = item.color;
            ctx.lineWidth = item.lineWidth || 3;

            if (item.mode === "points") {
                item.points.forEach(point => {
                    ctx.beginPath();
                    ctx.arc(sx(point.x), sy(point.y), 5, 0, Math.PI * 2);
                    ctx.fill();
                });
                return;
            }

            ctx.beginPath();
            item.points.forEach((point, index) => {
                const x = sx(point.x);
                const y = sy(point.y);
                if (index === 0) {
                    ctx.moveTo(x, y);
                } else {
                    ctx.lineTo(x, y);
                }
            });
            ctx.stroke();
        });
    }

    function drawAll() {
        const data = window.numericData || {};
        drawChart("taylorChart", [
            { points: data.taylor && data.taylor.real, color: "#15583b", lineWidth: 3 },
            { points: data.taylor && data.taylor.approx, color: "#f0b429", lineWidth: 3 }
        ]);
        drawChart("leastSquaresChart", [
            { points: data.leastSquares && data.leastSquares.points, color: "#15583b", mode: "points" },
            { points: data.leastSquares && data.leastSquares.line, color: "#f0b429", lineWidth: 3 }
        ]);
    }

    window.addEventListener("resize", drawAll);
    drawAll();
})();
