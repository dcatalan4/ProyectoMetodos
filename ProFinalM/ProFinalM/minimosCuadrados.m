clear
clc
close all


X = [1 4 5 6 10 12 14];
Y = [2 4 5 6 8 11 12];


m = length(X);


sumX  = sum(X);
sumY  = sum(Y);
sumXY = sum(X .* Y);
sumX2 = sum(X.^2);
sumY2 = sum(Y.^2);


a1 = ((m * sumXY) - (sumX * sumY)) / ((m * sumX2) - (sumX^2));

a0 = ((sumY * sumX2) - (sumX * sumXY)) / ((m * sumX2) - (sumX^2));


r = ((m * sumXY) - (sumX * sumY)) / ...
    sqrt(((m * sumX2) - (sumX^2)) * ((m * sumY2) - (sumY^2)));


R2 = r^2;


fprintf('Ecuacion de ajuste:\n');
fprintf('y = %.4fx + %.4f\n', a1, a0);

fprintf('Coeficiente de correlacion: %.4f\n', r);
fprintf('Coeficiente de determinacion R^2: %.4f\n', R2);


xModelo = linspace(min(X), max(X), 100);

yModelo = a1 * xModelo + a0;


figure

scatter(X, Y, 80, 'filled')
hold on

plot(xModelo, yModelo, 'LineWidth', 2)

grid on

title('Ajuste lineal por Minimos Cuadrados')
xlabel('Variable X')
ylabel('Variable Y')

legend('Datos', 'Recta de ajuste')