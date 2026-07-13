import { Component } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { ButtonModule } from "primeng/button";
import { TagModule } from "primeng/tag";
import { TimelineModule } from "primeng/timeline";
import { AccordionModule } from "primeng/accordion";

@Component({
  selector: "app-tax-info",
  standalone: true,
  templateUrl: "./tax.component.html",
  styleUrls: ["./tax.component.css"],
  imports: [
    CommonModule,
    RouterModule,
    ButtonModule,
    TagModule,
    TimelineModule,
    AccordionModule,
  ],
})
export class TaxInfoComponent {
  facts = [
    {
      icon: "pi pi-money-bill",
      title: "Income Tax",
      description:
        "Your employer normally deducts tax from your salary before it is paid.",
    },
    {
      icon: "pi pi-file-check",
      title: "Annual Tax Return",
      description:
        "Every year you'll receive a tax declaration from Skatteverket to review.",
    },
    {
      icon: "pi pi-wallet",
      title: "Refunds",
      description:
        "If you've paid too much tax you'll receive a refund directly to your registered bank account.",
    },
  ];

  steps = [
    {
      title: "Start Working",
      description:
        "Your employer reports your salary and deducts preliminary tax every month.",
    },
    {
      title: "Receive Tax Declaration",
      description:
        "Skatteverket prepares your annual tax return using information from employers and banks.",
    },
    {
      title: "Review Your Information",
      description:
        "Check your income, deductions and personal details before submitting.",
    },
    {
      title: "Submit Your Declaration",
      description:
        "Approve your declaration using BankID, the Skatteverket app or the online service.",
    },
    {
      title: "Receive Refund or Pay Remaining Tax",
      description:
        "After processing you'll either receive a refund or pay any remaining balance.",
    },
  ];

  deductions = [
    {
      icon: "pi pi-car",
      title: "Travel to Work",
      description:
        "Travel expenses may be deductible if you meet Skatteverket's requirements.",
    },
    {
      icon: "pi pi-home",
      title: "Double Residence",
      description:
        "Some people temporarily living away from home for work can claim deductions.",
    },
    {
      icon: "pi pi-briefcase",
      title: "Work Expenses",
      description: "Certain work-related costs may qualify for deductions.",
    },
    {
      icon: "pi pi-chart-line",
      title: "Investment Losses",
      description: "Some investment losses can reduce your taxable income.",
    },
    {
      icon: "pi pi-building",
      title: "Business Expenses",
      description:
        "Self-employed individuals can deduct eligible business costs.",
    },
    {
      icon: "pi pi-book",
      title: "Education",
      description:
        "Some education expenses connected to your profession may qualify.",
    },
  ];

  filingMethods = [
    {
      icon: "pi pi-mobile",
      title: "BankID",
      description:
        "The fastest and easiest way to submit your tax declaration.",
    },
    {
      icon: "pi pi-mobile",
      title: "Skatteverket App",
      description:
        "Review and approve your declaration directly from your phone.",
    },
    {
      icon: "pi pi-desktop",
      title: "Online Service",
      description: "Use Skatteverket's secure e-service from your computer.",
    },
    {
      icon: "pi pi-file",
      title: "Paper Form",
      description:
        "Paper declarations are available if you cannot file digitally.",
    },
  ];

  calendar = [
    {
      title: "March",
      description: "Tax declarations become available for most taxpayers.",
    },
    {
      title: "April",
      description:
        "Most people submit their tax declaration before the deadline.",
    },
    {
      title: "June",
      description: "Early tax refunds are paid to eligible taxpayers.",
    },
    {
      title: "August",
      description: "Additional tax refunds are processed.",
    },
    {
      title: "December",
      description: "Final tax payments and adjustments are completed.",
    },
  ];

  services = [
    {
      icon: "pi pi-file-edit",
      title: "Tax Return",
      description: "Review and submit your annual tax declaration.",
      button: "Open",
    },
    {
      icon: "pi pi-wallet",
      title: "Tax Account",
      description: "View payments, refunds and balances.",
      button: "View",
    },
    {
      icon: "pi pi-building-columns",
      title: "Register Bank Account",
      description: "Register your account for faster tax refunds.",
      button: "Register",
    },
    {
      icon: "pi pi-calculator",
      title: "Tax Calculator",
      description: "Estimate your Swedish income tax.",
      button: "Calculate",
    },
    {
      icon: "pi pi-percentage",
      title: "Deduction Guide",
      description: "Learn about common deductions you may qualify for.",
      button: "Learn More",
    },
    {
      icon: "pi pi-folder-open",
      title: "Forms & Certificates",
      description: "Download forms and official certificates.",
      button: "Open",
    },
  ];

  faqs = [
    {
      question: "Do I need to submit a tax return every year?",
      answer:
        "Most people living and working in Sweden receive an annual tax declaration from Skatteverket that must be reviewed and approved.",
    },
    {
      question: "Can I file my tax return in English?",
      answer:
        "Skatteverket provides extensive English guidance, although parts of the digital services remain in Swedish.",
    },
    {
      question: "Do I need BankID?",
      answer:
        "No, but BankID makes filing your tax return much faster and easier.",
    },
    {
      question: "What happens if I miss the deadline?",
      answer:
        "Late submissions may result in a late filing penalty from Skatteverket.",
    },
    {
      question: "How do I receive my tax refund?",
      answer:
        "Register your Swedish bank account with Skatteverket so refunds can be paid automatically.",
    },
  ];
}
