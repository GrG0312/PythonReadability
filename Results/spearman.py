import csv
import json
import sys
from scipy.stats import spearmanr

def load_report(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def compute_correlations(report: dict) -> list[dict]:
    results = []

    for model_result in report["modelResults"]:
        model_name = model_result["modelName"]
        concrete_answers = model_result["concreteAnswers"]

        original_scores = [a["originalScore"] for a in concrete_answers]
        model_answers = [a["modelAnswer"] for a in concrete_answers]

        correlation, p_value = spearmanr(original_scores, model_answers)

        results.append({
            "modelName": model_name,
            "spearmanCorrelation": correlation,
            "pValue": p_value,
            "significant": p_value < 0.05,
            "sampleSize": len(concrete_answers)
        })

    # Sort by correlation, strongest first
    results.sort(key=lambda r: r["spearmanCorrelation"], reverse=True)
    return results


def save_results_csv(results: list[dict], output_path: str) -> None:
    with open(output_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=["modelName", "spearmanCorrelation", "pValue", "significant", "sampleSize"])
        writer.writeheader()
        writer.writerows(results)


def main():
    if len(sys.argv) != 2:
        print("Usage: python spearman_correlation.py <report.json>")
        sys.exit(1)

    report_path = sys.argv[1]
    report = load_report(report_path)
    results = compute_correlations(report)

    output_path = report_path.rsplit(".", 1)[0] + "_spearman.csv"
    save_results_csv(results, output_path)

    print(f"Spearman correlation results for {len(results)} models saved to: {output_path}")


if __name__ == "__main__":
    main()