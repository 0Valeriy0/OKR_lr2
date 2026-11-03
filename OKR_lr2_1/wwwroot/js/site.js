document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');
    const hiddenExpression = document.getElementById('hiddenExpression');
    const serverResult = document.getElementById('serverResult');

    if (!display || !keypad) return;

    const operators = ['+', '-', '*', '/', '%'];
    let currentExpression = serverResult && serverResult.value ? serverResult.value : '0';

    function isSpecialState(expr) {
        return expr === '0' 
            || expr === 'Помилка' 
            || expr.startsWith('Для роботи з пам')
            || expr === 'Вираз не може бути порожнім';
    }

    function updateDisplay() {
        display.innerText = currentExpression;

        // Оновлюємо поле для стандартного обчислення (=)
        if (hiddenExpression) {
            hiddenExpression.value = currentExpression;
        }

        // Оновлюємо приховані поля з поточним виразом для RecallClearMemory, MemoryPlus, MemoryMinus
        document.querySelectorAll('.hidden-expression-input').forEach(input => {
            input.value = currentExpression;
        });

        // Заповнюємо значення для форми пам'яті лише якщо це одиночне число
        document.querySelectorAll('.hidden-value').forEach(input => {
            const trimmed = currentExpression.trim().replace(',', '.');
            if (/^[+-]?\d+(\.\d+)?$/.test(trimmed)) {
                input.value = parseFloat(trimmed);
            } else {
                input.value = '';
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

        const inSpecial = isSpecialState(currentExpression);

        if (val === 'backspace') {
            if (currentExpression.length > 1 && !inSpecial) {
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

        // Обмеження: вводити можна лише якщо кількість символів менше 16
        if (currentExpression.length >= 16 && !inSpecial) {
            return;
        }

        const lastChar = currentExpression.slice(-1);

        // Захист від подвійних операторів підряд (включаючи %)
        if (operators.includes(val)) {
            // Виняток: якщо це мінус як перший символ або після дужки (від'ємне число)
            if (val === '-' && (inSpecial || lastChar === '(')) {
                if (inSpecial) {
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
            if (operators.includes(lastChar) || lastChar === '(' || inSpecial) {
                if (inSpecial) {
                    currentExpression = '0.';
                } else {
                    currentExpression += '0.';
                }
                updateDisplay();
                return;
            }
        }

        // Обробка початкового стану "0", помилки або повідомлення
        if (inSpecial) {
            if (val === '.') {
                currentExpression = '0.';
            } else if (operators.includes(val)) {
                currentExpression = '0' + val;
            } else {
                currentExpression = val;
            }
        } else {
            currentExpression += val;
        }

        updateDisplay();
    }
});