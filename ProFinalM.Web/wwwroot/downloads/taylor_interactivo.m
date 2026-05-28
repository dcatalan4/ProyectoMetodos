clc;
clear;
close all;

syms x

fprintf('=== Serie de Taylor ===\n\n');

try

    
    fprintf('Ingrese la funcion en terminos de x\n');
    fStr = input('f(x) = ', 's');

    f = str2sym(fStr);

    
    a = input('Punto de expansion a: ');

    
    n = input('Orden del polinomio n: ');


    h = input('Paso h: ');

   
    if n < 0
        error('El orden no puede ser negativo');
    end

   
    xi_1 = a + h;


    dominio = [a - 5, a + 5];

 
    T = taylor(f, x, 'ExpansionPoint', a, 'Order', n+1);


    valorAproximado = double(subs(T, x, xi_1));
    valorReal       = double(subs(f, x, xi_1));

   
    Et = abs(valorReal - valorAproximado);
    Ep = abs((Et / valorReal) * 100);

  
    fprintf('\n--- Resultados ---\n');

    fprintf('Polinomio de Taylor:\n%s\n\n', char(T));

    fprintf('Valor Real: %.6f\n', valorReal);
    fprintf('Valor Aproximado: %.6f\n', valorAproximado);
    fprintf('Error Absoluto: %.6f\n', Et);
    fprintf('Error Porcentual: %.4f%%\n', Ep);

 
    figure

    fplot(f, dominio, 'b', 'LineWidth', 2)
    hold on

    fplot(T, dominio, 'r--', 'LineWidth', 2)

    scatter(xi_1, valorReal, 80, 'filled')

    grid on

    title('Serie de Taylor')
    xlabel('x')
    ylabel('y')

    legend('Funcion Real', 'Taylor', 'Punto evaluado')

catch ME

    fprintf('\nERROR: %s\n', ME.message);

end
