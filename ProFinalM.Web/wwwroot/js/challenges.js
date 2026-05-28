(function () {
    const bannedWords = [
        "cerote", "mierda", "puta", "puto", "pendejo", "pendeja", "idiota", "imbecil",
        "maldito", "maldita", "culero", "culera", "verga", "hueco", "joder", "fuck",
        "shit", "bitch", "asshole"
    ];

    const questions = [
        {
            text: "Que busca la Serie de Taylor?",
            options: ["Aproximar funciones con polinomios", "Ordenar datos alfabeticamente", "Eliminar todos los errores", "Convertir imagenes en tablas"],
            answer: 0
        },
        {
            text: "En Taylor, que se evalua en el punto a?",
            options: ["Solo el resultado final", "La funcion y sus derivadas", "Unicamente la pendiente de una recta", "El nombre del usuario"],
            answer: 1
        },
        {
            text: "Que representa a1 en Minimos Cuadrados?",
            options: ["El intercepto", "La pendiente", "El error porcentual", "El factorial"],
            answer: 1
        },
        {
            text: "Por que se elevan errores al cuadrado en Minimos Cuadrados?",
            options: ["Para hacer la tabla mas larga", "Para evitar cancelaciones entre errores positivos y negativos", "Para convertir x en y", "Para dibujar la grafica"],
            answer: 1
        },
        {
            text: "Que indica R cuadrado?",
            options: ["El grado del polinomio", "La calidad del ajuste del modelo", "El numero de jugadores", "El valor de pi"],
            answer: 1
        }
    ];

    let player = "";
    let index = 0;
    let score = 0;
    let answered = false;

    const playerName = document.getElementById("playerName");
    const startButton = document.getElementById("startChallenge");
    const nameMessage = document.getElementById("nameMessage");
    const activePlayer = document.getElementById("activePlayer");
    const scoreValue = document.getElementById("scoreValue");
    const totalValue = document.getElementById("totalValue");
    const progressValue = document.getElementById("progressValue");
    const questionCard = document.getElementById("questionCard");
    const answerOptions = document.getElementById("answerOptions");
    const answerFeedback = document.getElementById("answerFeedback");
    const betoToken = document.getElementById("betoToken");
    const scoreRows = document.getElementById("scoreRows");

    function normalizeName(value) {
        return value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase().replace(/[^a-z0-9]+/g, "");
    }

    function validateName(value) {
        const clean = value.trim().replace(/\s+/g, " ");
        if (clean.length < 3) {
            return "Escribe un nombre de al menos 3 caracteres.";
        }

        if (!/^[A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ _.-]+$/.test(clean)) {
            return "Usa solo letras, numeros, espacios, punto, guion y guion bajo.";
        }

        const normalized = normalizeName(clean);
        if (bannedWords.some(word => normalized.includes(word))) {
            return "Ese nombre no esta permitido. Usa un nombre respetuoso.";
        }

        return "";
    }

    function setMessage(text, kind) {
        nameMessage.textContent = text;
        nameMessage.dataset.kind = kind || "";
    }

    function renderQuestion() {
        answered = false;
        totalValue.textContent = questions.length;
        scoreValue.textContent = score;
        progressValue.textContent = `${Math.round((index / questions.length) * 100)}%`;
        betoToken.style.left = `${Math.min(94, (score / questions.length) * 94)}%`;

        if (index >= questions.length) {
            finishChallenge();
            return;
        }

        const question = questions[index];
        questionCard.classList.remove("locked");
        questionCard.querySelector("h2").textContent = question.text;
        answerFeedback.textContent = "";
        answerOptions.innerHTML = "";

        question.options.forEach((option, optionIndex) => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "answer-option";
            button.textContent = option;
            button.addEventListener("click", () => chooseAnswer(optionIndex));
            answerOptions.appendChild(button);
        });
    }

    function chooseAnswer(optionIndex) {
        if (answered) {
            return;
        }

        answered = true;
        const question = questions[index];
        const buttons = [...answerOptions.querySelectorAll("button")];
        buttons.forEach((button, currentIndex) => {
            button.disabled = true;
            if (currentIndex === question.answer) {
                button.classList.add("correct");
            }
            if (currentIndex === optionIndex && optionIndex !== question.answer) {
                button.classList.add("wrong");
            }
        });

        if (optionIndex === question.answer) {
            score++;
            answerFeedback.textContent = "Correcto. Beto avanza una casilla.";
            answerFeedback.dataset.kind = "ok";
        } else {
            answerFeedback.textContent = "Casi. Revisa la explicacion y sigue intentando.";
            answerFeedback.dataset.kind = "warn";
        }

        index++;
        window.setTimeout(renderQuestion, 950);
    }

    async function finishChallenge() {
        progressValue.textContent = "100%";
        betoToken.style.left = `${Math.min(94, (score / questions.length) * 94)}%`;
        questionCard.querySelector("h2").textContent = `Reto completado: ${score}/${questions.length}`;
        answerOptions.innerHTML = "";
        answerFeedback.textContent = "Guardando punteo...";
        answerFeedback.dataset.kind = "";

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const response = await fetch("/Home/SaveScore", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify({
                    userName: player,
                    score,
                    total: questions.length,
                    challenge: "Retos de Beto"
                })
            });

            const result = await response.json();
            if (!response.ok || !result.ok) {
                throw new Error(result.message || "No se pudo guardar el punteo.");
            }

            answerFeedback.textContent = "Punteo guardado correctamente.";
            answerFeedback.dataset.kind = "ok";
            renderScores(result.scores);
        } catch (error) {
            answerFeedback.textContent = error.message;
            answerFeedback.dataset.kind = "warn";
        }
    }

    function renderScores(scores) {
        if (!scores || !scores.length) {
            scoreRows.innerHTML = '<tr><td colspan="4">Todavia no hay punteos guardados.</td></tr>';
            return;
        }

        scoreRows.innerHTML = scores.map(item => {
            const date = new Date(item.createdAt);
            return `<tr>
                <td>${escapeHtml(item.userName)}</td>
                <td>${item.score}/${item.total}</td>
                <td>${escapeHtml(item.challenge)}</td>
                <td>${date.toLocaleString()}</td>
            </tr>`;
        }).join("");
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    startButton.addEventListener("click", () => {
        const cleanName = playerName.value.trim().replace(/\s+/g, " ");
        const error = validateName(cleanName);
        if (error) {
            setMessage(error, "warn");
            return;
        }

        player = cleanName;
        index = 0;
        score = 0;
        activePlayer.textContent = player;
        setMessage("Reto iniciado. Buena suerte.", "ok");
        renderQuestion();
    });

    playerName.addEventListener("input", () => {
        const error = validateName(playerName.value);
        setMessage(error, error ? "warn" : "");
    });
})();
