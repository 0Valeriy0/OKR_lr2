document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');
    const hiddenExpression = document.getElementById('hiddenExpression');
    const serverResult = document.getElementById('serverResult');

    if (!display || !keypad) return;

    const operators = ['+', '-', '*', '/', '%'];
    let currentExpression = serverResult && serverResult.value ? serverResult.value : '0';

    function updateDisplay() {
        display.innerText = currentExpression;

        // Оновлюємо поле для стандартного обчислення (=)
        if (hiddenExpression) {
            hiddenExpression.value = currentExpression;
        }

        // Оновлюємо поля для M+ та M-, намагаючись перетворити поточний вираз на число
        document.querySelectorAll('.hidden-value').forEach(input => {
            try {
                // Якщо на екрані "2+2", воно порахує 4, щоб відправити в контролер (double value)
                input.value = eval(currentExpression) || 0;
            } catch (e) {
                input.value = 0;
            }
        });
    }

    updateDisplay();

    keypad.addEventListener('click', (event) => {
        const btn = event.target.closest('button');
        if (!btn) return;

        const val = btn.dataset.val;
        if (!val) return;

        // Забороняємо стандартну дію лише для кнопок, які НЕ відправляють форму на сервер
        if (btn.type !== 'submit') {
            event.preventDefault();
        }

        handleInput(val);
    });

    function handleInput(val) {
        if (val === 'CE') {
            currentExpression = '0';
            updateDisplay();
            return;
        }

        if (val === 'backspace') {
            if (currentExpression.length > 1 && currentExpression !== 'Помилка') {
                currentExpression = currentExpression.slice(0, -1);
            } else {
                currentExpression = '0';
            }
            updateDisplay();
            return;
        }

        // Якщо це кнопка форми (=, M+, M-, R-CM, CA), браузер сам відправить запит на сервер
        if (['=', 'M+', 'M-', 'R-CM', 'CA'].includes(val)) {
            return;
        }

        // Ігноруємо решту ще не реалізованих кнопок Sharp
        if (['prev', 'next', 'auto-replay', 'correct', 'tax-plus', 'tax-minus', 'rate'].includes(val)) {
            return;
        }

        const lastChar = currentExpression.slice(-1);

        // Захист від подвійних операторів підряд (включаючи %)
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
        }

        // Валідація крапки: перевіряємо, чи є крапка в ПОТОЧНОМУ числі
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

        // Обробка початкового стану "0" або "Помилка"
        if (currentExpression === '0' || currentExpression === 'Помилка') {
            if (val === '.') {
                currentExpression = '0.';
            } else if (operators.includes(val)) {
                currentExpression += val;
            } else {
                currentExpression = val;
            }
        } else {
            currentExpression += val;
        }

        updateDisplay();
    }
});