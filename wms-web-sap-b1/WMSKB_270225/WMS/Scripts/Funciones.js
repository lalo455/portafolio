const trunc = (n, decimalPlaces) => {
    const decimals = decimalPlaces ? decimalPlaces : 5;
    const asString = n.toString();
    const pos = asString.indexOf('.') != -1 ? asString.indexOf('.') + decimals + 1 : asString.length;
    return parseFloat(n.toString().substring(0, pos));
};