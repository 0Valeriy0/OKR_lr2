document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');

    if (!display || !keypad) return;

    let currentExpression = '0';
    // 1. Додано відсоток '%' до списку операторів
    const operators = ['+', '-', '*', '/', '%'];

    function updateDisplay() {
        display.innerText = currentExpression;
    }

    updateDisplay();

    keypad.addEventListener('click', (event) => {
        const btn = event.target.closest('button');
        if (!btn) return;

        const val = btn.dataset.val;
        if (!val) return;

        handleInput(val);
    });

    function handleInput(val) {
        // 1. Очищення екрана (CE або CA)
        if (val === 'CE' || val === 'CA') {
            currentExpression = '0';
            updateDisplay();
            return;
        }

        // 2. Стерти один символ назад
        if (val === 'backspace') {
            if (currentExpression.length > 1) {
                currentExpression = currentExpression.slice(0, -1);
            } else {
                currentExpression = '0';
            }
            updateDisplay();
            return;
        }

        // 3. Знак "=" (дозволяємо стандартну відправку форми на бекенд)
        if (val === '=') {
            return;
        }

        // 4. Ігноруємо службові кнопки Sharp
        if (['prev', 'next', 'auto-replay', 'correct', 'tax-plus', 'tax-minus', 'rate', 'R-CM', 'M-', 'M+'].includes(val)) {
            return;
        }

        const lastChar = currentExpression.slice(-1);

        // 5. Захист від подвійних операторів підряд (включаючи %)
        if (operators.includes(val)) {
            // Виняток: якщо це мінус як перший символ або після дужки (від'ємне число)
            if (val === '-' && (currentExpression === '0' || lastChar === '(')) {
                if (currentExpression === '0') {
                    currentExpression = '-';
                } else {
                    currentExpression += '-';
                }
                updateDisplay();
                return;
            }

            // Якщо попередній символ теж оператор — замінюємо його
            if (operators.includes(lastChar)) {
                // Якщо на початку було "- ", не міняємо його на "*" чи "/"
                if (currentExpression === '-' && val !== '-') return;

                currentExpression = currentExpression.slice(0, -1) + val;
                updateDisplay();
                return;
            }
        }

        // 6. Валідація крапки: перевіряємо, чи є крапка в ПОТОЧНОМУ числі
        if (val === '.') {
            // Розбиваємо вираз за операторами та дужками, щоб витягнути остання введене число
            const tokens = currentExpression.split(/[\+\-\*\/\%\(\)]/);
            const currentNumber = tokens[tokens.length - 1];

            // Якщо в поточному числі ВЖЕ Є крапка — ігноруємо ввід (захист від 3.4.5)
            if (currentNumber.includes('.')) {
                return;
            }

            // Якщо крапку тиснуть одразу після операції — додаємо "0."
            if (operators.includes(lastChar)) {
                currentExpression += '0.';
                updateDisplay();
                return;
            }
        }

        // 7. Обробка початкового стану "0" або унарного мінуса "-"
        if (currentExpression === '0') {
            if (val === '.') {
                currentExpression = '0.';
            } else if (val === '-') {
                currentExpression = '-'; // Від'ємне число на початку
            } else if (operators.includes(val)) {
                currentExpression += val; // "0+"
            } else {
                currentExpression = val;
            }
        } else {
            currentExpression += val;
        }

        updateDisplay();
    }
});