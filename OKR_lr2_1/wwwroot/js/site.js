document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');
    const hiddenExpression = document.getElementById('hiddenExpression');
    const serverResult = document.getElementById('serverResult');

    if (!display || !keypad) return;

    const operators = ['+', '-', '*', '/'];

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
                // Якщо на екрані "2+2", воно спочатку порахує 4, щоб відправити в Controller(double value)
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

        // Якщо це кнопка форми (=, M+, M-, R-CM, CA), браузер сам відправить запит
        if (['=', 'M+', 'M-', 'R-CM', 'CA'].includes(val)) {
            return;
        }

        // Ігноруємо решту ще не реалізованих кнопок Sharp
        if (['prev', 'next', 'auto-replay', 'correct', 'tax-plus', 'tax-minus', 'rate'].includes(val)) {
            return;
        }

        const lastChar = currentExpression.slice(-1);

        if (operators.includes(val)) {
            if (operators.includes(lastChar)) {
                currentExpression = currentExpression.slice(0, -1) + val;
                updateDisplay();
                return;
            }
        }

        if (val === '.') {
            if (lastChar === '.') return;
            if (operators.includes(lastChar)) {
                currentExpression += '0.';
                updateDisplay();
                return;
            }
        }

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