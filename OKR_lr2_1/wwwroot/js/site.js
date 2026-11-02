document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');

    if (!display || !keypad) return;

    let currentExpression = '0';
    const operators = ['+', '-', '*', '/', '%'];

    function updateDisplay() {
        display.innerText = currentExpression;
    }

    // Встановлюємо 0 при завантаженні
    updateDisplay();

    // Слухаємо кліки по кнопках
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

        // 2. Стерти один символ назад (стрілка →)
        if (val === 'backspace') {
            if (currentExpression.length > 1) {
                currentExpression = currentExpression.slice(0, -1);
            } else {
                currentExpression = '0';
            }
            updateDisplay();
            return;
        }

        // 3. Знак "=" (ігноруємо для виводу)
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

            // Якщо попередній символ теж оператор — замінюємо його на новий
            if (operators.includes(lastChar)) {
                currentExpression = currentExpression.slice(0, -1) + val;
                updateDisplay();
                return;
            }
        } // <- Тут дужка була пропущена!

        // 6. Валідація крапки: перевіряємо, чи є крапка в ПОТОЧНОМУ числі
        if (val === '.') {
            const tokens = currentExpression.split(/[\+\-\*\/\%\(\)]/);
            const currentNumber = tokens[tokens.length - 1];

            // Якщо в поточному числі ВЖЕ Є крапка — ігноруємо ввід
            if (currentNumber.includes('.')) {
                return;
            }

            // Якщо крапку тиснуть одразу після операції або дужки — додаємо "0."
            if (operators.includes(lastChar) || lastChar === '(' || currentExpression === '0') {
                if (currentExpression === '0') {
                    currentExpression = '0.';
                } else {
                    currentExpression += '0.';
                }
                updateDisplay();
                return;
            }
        }

        // 7. Обробка початкового стану "0"
        if (currentExpression === '0') {
            if (val === '.') {
                currentExpression = '0.';
            } else if (operators.includes(val)) {
                currentExpression += val; // "0+"
            } else {
                currentExpression = val; // заміна "0" на цифру чи дужку
            }
        } else {
            currentExpression += val;
        } // <- Тут дужка була пропущена!

        updateDisplay();
    }
});