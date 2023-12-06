import { fail } from "assert";
import { readFileSync } from "fs";

const token = process.argv[2].trim();
const pullNumber = process.argv[3].trim();

// Add a comment to the pull request
import { Octokit, App } from "octokit";
const gh = new Octokit({ auth: token });

console.log(`Reporting test results for pull request ${pullNumber}`);
console.log(`Using token ${token}`);

const results = JSON.parse(readFileSync("test-results.json", "utf8")) as TestResults;

// Get all the failed tests
const failedSpecs = results.suites.map((suite: Suite) => suite.specs.filter((spec: Spec) => !spec.ok)).flat();

gh.rest.pulls.createReview({
    owner: "somfic",
    repo: "vla",
    pull_number: parseInt(pullNumber),
    body: "e2e test results",
    //comments: failedSpecs.map((spec) => ({ path: spec.file, position: 0, body: specToBody(spec) })),
    event: failedSpecs.length > 0 ? "REQUEST_CHANGES" : "APPROVE",
});

function specToBody(spec: Spec) {
    const test = spec.tests[0];
    const result = test.results[0];
    return `Test **${spec.title} failed**\n\n${result.error?.message}`;
}

export interface TestResults {
    config: Config;
    suites: Suite[];
    errors: any[];
    stats: Stats;
}

export interface Config {
    configFile: string;
    rootDir: string;
    forbidOnly: boolean;
    fullyParallel: boolean;
    globalSetup: any;
    globalTeardown: any;
    globalTimeout: number;
    grep: Grep;
    grepInvert: any;
    maxFailures: number;
    metadata: Metadata;
    preserveOutput: string;
    reporter: [string, Reporter | undefined][];
    reportSlowTests: ReportSlowTests;
    quiet: boolean;
    projects: Project[];
    shard: any;
    updateSnapshots: string;
    version: string;
    workers: number;
    webServer: any;
}

export interface Grep {}

export interface Metadata {
    actualWorkers: number;
}

export interface Reporter {
    outputFile: string;
}

export interface ReportSlowTests {
    max: number;
    threshold: number;
}

export interface Project {
    outputDir: string;
    repeatEach: number;
    retries: number;
    id: string;
    name: string;
    testDir: string;
    testIgnore: any[];
    testMatch: string[];
    timeout: number;
}

export interface Suite {
    title: string;
    file: string;
    column: number;
    line: number;
    specs: Spec[];
}

export interface Spec {
    title: string;
    ok: boolean;
    tags: any[];
    tests: Test[];
    id: string;
    file: string;
    line: number;
    column: number;
}

export interface Test {
    timeout: number;
    annotations: any[];
    expectedStatus: string;
    projectId: string;
    projectName: string;
    results: Result[];
    status: string;
}

export interface Result {
    workerIndex: number;
    status: string;
    duration: number;
    errors: Error[];
    stdout: any[];
    stderr: any[];
    retry: number;
    startTime: string;
    attachments: any[];
    error?: Error2;
    errorLocation?: ErrorLocation;
}

export interface Error {
    location: Location;
    message: string;
}

export interface Location {
    file: string;
    column: number;
    line: number;
}

export interface Error2 {
    message: string;
    stack: string;
    location: Location2;
    snippet: string;
}

export interface Location2 {
    file: string;
    column: number;
    line: number;
}

export interface ErrorLocation {
    file: string;
    column: number;
    line: number;
}

export interface Stats {
    startTime: string;
    duration: number;
    expected: number;
    skipped: number;
    unexpected: number;
    flaky: number;
}
