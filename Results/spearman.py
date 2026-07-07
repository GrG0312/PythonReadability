#!/usr/bin/env python3

import sys
import json
from scipy.stats import spearmanr


def compute_correlations(report_path: str):
    with open(report_path, "r", encoding="utf-8") as f:
        report = json.load(f)

    results = []

    for model in report["modelResults"]:
        name = model["modelName"]
        answers = model["concreteAnswers"]

        original_scores = [a["originalScore"] for a in answers]
        model_answers = [a["modelAnswer"] for a in answers]

        correlation, p_value = spearmanr(original_scores, model_answers)

        results.append({
            "modelName": name,
            "spearmanCorrelation": correlation,
            "pValue": p_value,
            "sampleSize": len(answers)
        })

    return results


def main():
    if len(sys.argv) != 2:
        print("Usage: python spearman_correlation.py <report.json>")
        sys.exit(1)

    report_path = sys.argv[1]
    results = compute_correlations(report_path)

    # Sort by correlation descending, best model first
    results.sort(key=lambda r: r["spearmanCorrelation"], reverse=True)

    print(f"{'Model':<40} {'Spearman r':>12} {'p-value':>12} {'N':>6}")
    print("-" * 72)
    for r in results:
        print(f"{r['modelName']:<40} {r['spearmanCorrelation']:>12.4f} "
              f"{r['pValue']:>12.2e} {r['sampleSize']:>6}")


if __name__ == "__main__":
    main()