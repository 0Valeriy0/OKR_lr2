document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');

    if (!display || !keypad) return;

    let currentExpression = '0';
    const operators = ['+', '-', '*', '/'];

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

        // 3. Знак "=" НЕ додається на екран (зарезервовано під відправку на бекенд)
        if (val === '=') {
            // Тут буде відправка на бекенд контролера
            return;
        }

        // 4. Ігноруємо службові кнопки Sharp
        if (['prev', 'next', 'auto-replay', 'correct', 'tax-plus', 'tax-minus', 'rate', 'R-CM', 'M-', 'M+'].includes(val)) {
            return;
        }

        // Отримуємо останній символ, який зараз є на екрані
        const lastChar = currentExpression.slice(-1);

        // 5. Захист від двох знаків операцій поспіль (++, --, //, +- тощо)
        if (operators.includes(val)) {
            if (operators.includes(lastChar)) {
                // Замінюємо останній введений знак на щойно натиснутий
                currentExpression = currentExpression.slice(0, -1) + val;
                updateDisplay();
                return;
            }
        }

        // 6. Захист від кількох крапок підряд
        if (val === '.') {
            if (lastChar === '.') return;
            // Якщо натиснули крапку одразу після операції (+, -, *, /) — додаємо "0."
            if (operators.includes(lastChar)) {
                currentExpression += '0.';
                updateDisplay();
                return;
            }
        }

        // 7. Введення цифр, дужок та перших операцій
        if (currentExpression === '0') {
            if (val === '.') {
                currentExpression = '0.';
            } else if (operators.includes(val)) {
                currentExpression += val; // наприклад, дозволяємо "0+"
            } else {
                currentExpression = val; // замінюємо нуль на першу цифру чи дужку
            }
        } else {
            currentExpression += val;
        }

        updateDisplay();
    }
});