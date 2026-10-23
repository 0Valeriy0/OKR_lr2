document.addEventListener('DOMContentLoaded', () => {
    const display = document.getElementById('display');
    const keypad = document.querySelector('.keypad-grid');

    if (!display || !keypad) return;

    let currentExpression = '0';

    function updateDisplay() {
        display.innerText = currentExpression;
    }


    // Слухаємо кліки по кнопках
    keypad.addEventListener('click', (event) => {
        const btn = event.target.closest('button');
        if (!btn) return;

        const val = btn.dataset.val;
        if (!val) return;

        handleInput(val);
    });

    function handleInput(val) {
        // 1. Очищення екрана
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

        // 3. Ігноруємо службові кнопки Sharp
        if (['prev', 'next', 'auto-replay', 'correct', 'tax-plus', 'tax-minus', 'rate', 'R-CM', 'M-', 'M+'].includes(val)) {
            return;
        }

        // 4. Виведення натиснутих кнопок на дисплей
        if (currentExpression === '0') {
            if (val === '.') {
                currentExpression = '0.';
            } else if (['+', '-', '*', '/'].includes(val)) {
                currentExpression += val;
            } else {
                currentExpression = val; // замінюємо початковий 0 на першу цифру
            }
        } else {
            // Захист від кількох крапок підряд
            if (val === '.' && currentExpression.endsWith('.')) return;
            currentExpression += val;
        }

        updateDisplay();
    }
});